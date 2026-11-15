namespace ShortLink.Api.Services;

public static class UrlValidator
{
    public static bool IsValid(string? value) =>
        value is { Length: > 0 and <= 2048 }
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && !string.IsNullOrEmpty(uri.Host)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
