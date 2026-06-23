namespace CzechNewsMap.Api.Services;

public class IrozhlasSourceService : RssFeedSourceService
{
    public IrozhlasSourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://www.irozhlas.cz/rss/irozhlas";
    protected override string SourceName => "iROZHLAS";
}