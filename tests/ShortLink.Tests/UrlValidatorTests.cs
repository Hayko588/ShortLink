using ShortLink.Api.Services;

namespace ShortLink.Tests;

public sealed class UrlValidatorTests
{
    [Theory]
    [InlineData("https://example.com/some/long/path", true)]
    [InlineData("http://localhost:8080/path", true)]
    [InlineData("http:example.com/path", false)]
    [InlineData("ftp://example.com/file", false)]
    [InlineData("/relative/path", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_AcceptsOnlyAbsoluteHttpUrls(string? url, bool expected) =>
        Assert.Equal(expected, UrlValidator.IsValid(url));

    [Fact]
    public void IsValid_RejectsUrlsLongerThan2048Characters() =>
        Assert.False(UrlValidator.IsValid($"https://example.com/{new string('a', 2048)}"));

    [Fact]
    public void IsValid_AcceptsUrlAtMaximumLength()
    {
        const string prefix = "https://example.com/";
        var url = prefix + new string('a', 2048 - prefix.Length);

        Assert.True(UrlValidator.IsValid(url));
    }
}
