namespace CzechNewsMap.Api.Services;

public class SeznamZpravySourceService : RssFeedSourceService
{
    public SeznamZpravySourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://www.seznamzpravy.cz/rss/domaci";
    protected override string SourceName => "Seznam Zprávy";
}