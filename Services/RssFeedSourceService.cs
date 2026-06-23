using System.ServiceModel.Syndication;
using System.Xml;
using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public abstract class RssFeedSourceService : ISourceService
{
    private readonly HttpClient _httpClient;

    protected RssFeedSourceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "CzechNewsMap/1.0");
    }

    protected abstract string FeedUrl { get; }
    protected abstract string SourceName { get; }
    protected virtual int MaxItems => 30;

    public async Task<List<SourceArticle>> GetArticlesAsync()
    {
        using var stream = await _httpClient.GetStreamAsync(FeedUrl);
        using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });

        var feed = SyndicationFeed.Load(xmlReader);

        if (feed == null)
        {
            return new List<SourceArticle>();
        }

        return feed.Items
            .Take(MaxItems)
            .Select(item => new SourceArticle
            {
                Title = item.Title?.Text ?? "",
                Link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? "",
                PublishedAt = item.PublishDate != DateTimeOffset.MinValue
                    ? item.PublishDate.UtcDateTime
                    : DateTime.UtcNow,
                SourceName = SourceName,
                Summary = item.Summary?.Text ?? item.Content?.ToString() ?? ""
            })
            .Where(article => !string.IsNullOrWhiteSpace(article.Title))
            .ToList();
    }
}