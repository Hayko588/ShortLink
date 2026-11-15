using Microsoft.EntityFrameworkCore;
using Prometheus;
using ShortLink.Api.Data;
using ShortLink.Api.Models;
using ShortLink.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://+:8080");
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddDbContext<ShortLinkDbContext>((services, options) =>
{
    var connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default must be configured.");
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("RUN_MIGRATIONS"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ShortLinkDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpMetrics();

app.MapPost("/links", async (CreateLinkRequest request, ShortLinkDbContext db, IConfiguration configuration) =>
{
    if (!UrlValidator.IsValid(request.Url))
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid URL",
            detail: "The URL must be an absolute HTTP or HTTPS URL no longer than 2048 characters.");
    }

    while (true)
    {
        var link = new Link
        {
            Code = CodeGenerator.Generate(),
            LongUrl = request.Url!,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Links.Add(link);

        try
        {
            await db.SaveChangesAsync();
            LinkMetrics.LinksCreated.Inc();

            var baseUrl = (configuration["BASE_URL"] ?? "http://localhost:8080").TrimEnd('/');
            var shortUrl = $"{baseUrl}/{link.Code}";
            return Results.Created(shortUrl, new CreateLinkResponse(link.Code, shortUrl, link.LongUrl, link.CreatedAt));
        }
        catch (DbUpdateException exception) when (IsCodeCollision(exception))
        {
            db.Entry(link).State = EntityState.Detached;
        }
    }
});

app.MapGet("/{code:length(7)}", async (string code, ShortLinkDbContext db) =>
{
    if (!CodeGenerator.IsValid(code))
    {
        return Results.NotFound();
    }

    var link = await db.Links.SingleOrDefaultAsync(item => item.Code == code);
    if (link is null)
    {
        LinkMetrics.Redirects.WithLabels("miss").Inc();
        return Results.NotFound();
    }

    LinkMetrics.Redirects.WithLabels("hit").Inc();
    return Results.Redirect(link.LongUrl);
});

app.MapGet("/health/live", () => Results.Ok());
app.MapGet("/health/ready", async (ShortLinkDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok()
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapMetrics("/metrics");

app.Run();

static bool IsCodeCollision(DbUpdateException exception) =>
    exception.InnerException is Npgsql.PostgresException
    {
        SqlState: Npgsql.PostgresErrorCodes.UniqueViolation,
        ConstraintName: "IX_Links_Code"
    };

public partial class Program;
