namespace CzechNewsMap.Api.Services;

public class DenikSourceService : RssFeedSourceService
{
    public DenikSourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://www.denik.cz/rss/zpravy.html";
    protected override string SourceName => "Deník.cz";
}