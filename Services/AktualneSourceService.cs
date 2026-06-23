namespace CzechNewsMap.Api.Services;

public class AktualneSourceService : RssFeedSourceService
{
    public AktualneSourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://www.aktualne.cz/rss/";
    protected override string SourceName => "Aktuálně.cz";
}