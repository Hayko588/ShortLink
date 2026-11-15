using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShortLink.Api.Data;
using ShortLink.Api.Models;

namespace ShortLink.Tests;

public sealed class ShortLinkApiTests
{
    [Fact]
    public async Task CreateThenRedirect_ReturnsSavedDestination()
    {
        await using var factory = new TestApiFactory(useInMemoryDatabase: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await factory.InitializeDatabaseAsync();

        var response = await client.PostAsJsonAsync("/links", new CreateLinkRequest("https://example.com/a/long/path"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateLinkResponse>();
        Assert.NotNull(created);
        Assert.Equal(7, created.Code.Length);
        Assert.Equal($"http://short.test/{created.Code}", created.ShortUrl);
        Assert.Equal("https://example.com/a/long/path", created.LongUrl);

        var redirect = await client.GetAsync($"/{created.Code}");

        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal("https://example.com/a/long/path", redirect.Headers.Location?.ToString());
    }

    [Fact]
    public async Task RedirectMiss_ReturnsNotFound()
    {
        await using var factory = new TestApiFactory(useInMemoryDatabase: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await factory.InitializeDatabaseAsync();

        var response = await client.GetAsync("/abc1234");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task InvalidUrl_ReturnsProblemDetails()
    {
        await using var factory = new TestApiFactory(useInMemoryDatabase: true);
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync();

        var response = await client.PostAsJsonAsync("/links", new CreateLinkRequest("javascript:alert(1)"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Readiness_ReturnsServiceUnavailableWhenDatabaseIsDown()
    {
        await using var factory = new TestApiFactory(useInMemoryDatabase: false);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Metrics_ExposeHttpAndShortLinkCounters()
    {
        await using var factory = new TestApiFactory(useInMemoryDatabase: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await factory.InitializeDatabaseAsync();

        var create = await client.PostAsJsonAsync("/links", new CreateLinkRequest("https://example.com/metrics-test"));
        var created = await create.Content.ReadFromJsonAsync<CreateLinkResponse>();
        Assert.NotNull(created);
        await client.GetAsync($"/{created.Code}");

        var metrics = await client.GetStringAsync("/metrics");

        Assert.Contains("http_requests_received_total", metrics);
        Assert.Contains("shortlink_links_created_total", metrics);
        Assert.Contains("shortlink_redirects_total{result=\"hit\"}", metrics);
    }
}

internal sealed class TestApiFactory(bool useInMemoryDatabase) : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"shortlink-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    "Host=127.0.0.1;Port=1;Database=shortlink;Username=test;Password=test;Timeout=1",
                ["BASE_URL"] = "http://short.test",
                ["RUN_MIGRATIONS"] = "false",
                ["ASPNETCORE_URLS"] = "http://localhost"
            });
        });

        if (useInMemoryDatabase)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ShortLinkDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ShortLinkDbContext>>();
                services.AddDbContext<ShortLinkDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ShortLinkDbContext>().Database.EnsureCreatedAsync();
    }
}
