using Prometheus;

namespace ShortLink.Api.Services;

public static class LinkMetrics
{
    public static readonly Counter LinksCreated = Metrics.CreateCounter(
        "shortlink_links_created_total",
        "Total number of links created.");

    public static readonly Counter Redirects = Metrics.CreateCounter(
        "shortlink_redirects_total",
        "Total number of redirect requests by result.",
        new CounterConfiguration { LabelNames = ["result"] });
}
