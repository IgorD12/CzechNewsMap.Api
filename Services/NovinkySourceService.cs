namespace CzechNewsMap.Api.Services;

public class NovinkySourceService : RssFeedSourceService
{
    public NovinkySourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://www.novinky.cz/rss";
    protected override string SourceName => "Novinky.cz";
}