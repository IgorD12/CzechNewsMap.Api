using System.ServiceModel.Syndication;
using System.Xml;
using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public class NovinkySourceService : ISourceService
{
    private readonly HttpClient _httpClient;

    private const string FeedUrl = "https://www.novinky.cz/rss/sekce/krimi";

    public NovinkySourceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SourceArticle>> GetArticlesAsync()
    {
        using var stream = await _httpClient.GetStreamAsync(FeedUrl);

        using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse
        });

        var feed = SyndicationFeed.Load(xmlReader);

        if (feed == null)
        {
            return new List<SourceArticle>();
        }

        return feed.Items
            .Take(30)
            .Select(item => new SourceArticle
            {
                Title = item.Title?.Text ?? "",
                Link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? "",
                PublishedAt = item.PublishDate != DateTimeOffset.MinValue
                    ? item.PublishDate.UtcDateTime
                    : DateTime.UtcNow,
                SourceName = "Novinky.cz",
                Summary = item.Summary?.Text ?? ""
            })
            .ToList();
    }
}