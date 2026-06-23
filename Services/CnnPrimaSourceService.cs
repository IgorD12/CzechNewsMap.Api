namespace CzechNewsMap.Api.Services;

public class CnnPrimaSourceService : RssFeedSourceService
{
    public CnnPrimaSourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://cnn.iprima.cz/rss";
    protected override string SourceName => "CNN Prima NEWS";
}