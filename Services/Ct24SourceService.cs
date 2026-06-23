namespace CzechNewsMap.Api.Services;

public class Ct24SourceService : RssFeedSourceService
{
    public Ct24SourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://ct24.ceskatelevize.cz/rss";
    protected override string SourceName => "ČT24";
}