namespace ShortLink.Api.Models;

public sealed class Link
{
    public long Id { get; set; }

    public required string Code { get; set; }

    public required string LongUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed record CreateLinkRequest(string? Url);

public sealed record CreateLinkResponse(string Code, string ShortUrl, string LongUrl, DateTimeOffset CreatedAt);

/// <summary>Represents a registered short link.</summary>
public sealed record LinkResponse(string Code, string ShortUrl, string LongUrl, DateTimeOffset CreatedAt);
