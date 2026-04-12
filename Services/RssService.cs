using System.ServiceModel.Syndication;
using System.Xml;
using CzechNewsMap.Api.Models;
using Microsoft.Extensions.Options;

namespace CzechNewsMap.Api.Services;

public class RssService
{
    private readonly HttpClient _httpClient;
    private readonly RssOptions _rssOptions;

    public RssService(HttpClient httpClient, IOptions<RssOptions> rssOptions)
    {
        _httpClient = httpClient;
        _rssOptions = rssOptions.Value;
    }

    public async Task<List<RssArticle>> GetLatestArticlesAsync(int maxItems = 10)
    {
        if (string.IsNullOrWhiteSpace(_rssOptions.FeedUrl))
        {
            throw new InvalidOperationException("RSS FeedUrl is not configured.");
        }

        using var stream = await _httpClient.GetStreamAsync(_rssOptions.FeedUrl);
        using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse
        });
        var feed = SyndicationFeed.Load(xmlReader);

        if (feed == null)
        {
            return new List<RssArticle>();
        }
        
        var articles = feed.Items
            .Take(maxItems)
            .Select(item => new RssArticle
            {
                Title = item.Title?.Text ?? "",
                Link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? "",
                PublishedAt = item.PublishDate != DateTimeOffset.MinValue
                    ? item.PublishDate.UtcDateTime
                    : DateTime.UtcNow,
                SourceName = _rssOptions.SourceName,
                Summary = item.Summary?.Text ?? ""
            })
            .ToList();

        return articles;
    }
}