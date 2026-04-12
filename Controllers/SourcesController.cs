using CzechNewsMap.Api.Models;
using CzechNewsMap.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CzechNewsMap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly IEnumerable<ISourceService> _sources;
    private readonly ArticleDedupService _dedupService;
    private readonly RssEventMapper _mapper;

    public SourcesController(
        IEnumerable<ISourceService> sources,
        ArticleDedupService dedupService,
        RssEventMapper mapper)
    {
        _sources = sources;
        _dedupService = dedupService;
        _mapper = mapper;
    }

    [HttpGet("events")]
[HttpGet("events")]
public async Task<ActionResult<List<NewsEvent>>> GetEvents()
{
    var allArticles = new List<SourceArticle>();

    foreach (var source in _sources)
    {
        var articles = await source.GetArticlesAsync();
        allArticles.AddRange(articles);
    }

    var deduplicated = _dedupService.Deduplicate(allArticles);

    var events = deduplicated
        .Select(a => _mapper.MapToEvent(new RssArticle
        {
            Title = a.Title,
            Link = a.Link,
            PublishedAt = a.PublishedAt,
            SourceName = a.SourceName,
            Summary = a.Summary
        }))
        .Where(e => e != null)
        .Cast<NewsEvent>()
        .ToList();

    return Ok(events);
}
}