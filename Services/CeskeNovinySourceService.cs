namespace CzechNewsMap.Api.Services;

public class CeskeNovinySourceService : RssFeedSourceService
{
    public CeskeNovinySourceService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string FeedUrl => "https://www.ceskenoviny.cz/sluzby/rss/zpravy.php";
    protected override string SourceName => "České noviny";
}