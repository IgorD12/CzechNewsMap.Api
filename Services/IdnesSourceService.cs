using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public class IdnesSourceService : ISourceService
{
    private readonly RssService _rssService;

    public IdnesSourceService(RssService rssService)
    {
        _rssService = rssService;
    }

    public async Task<List<SourceArticle>> GetArticlesAsync()
    {
        var rssArticles = await _rssService.GetLatestArticlesAsync(20);

        return rssArticles.Select(a => new SourceArticle
        {
            Title = a.Title,
            Link = a.Link,
            PublishedAt = a.PublishedAt,
            SourceName = a.SourceName,
            Summary = a.Summary
        }).ToList();
    }
}