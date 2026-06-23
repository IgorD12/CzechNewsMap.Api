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
    private readonly SourceDiagnosticsService _diagnosticsService;
    private readonly ILogger<SourcesController> _logger;

    public SourcesController(
        IEnumerable<ISourceService> sources,
        ArticleDedupService dedupService,
        RssEventMapper mapper,
        SourceDiagnosticsService diagnosticsService,
        ILogger<SourcesController> logger)
    {
        _sources = sources;
        _dedupService = dedupService;
        _mapper = mapper;
        _diagnosticsService = diagnosticsService;
        _logger = logger;
    }

    [HttpGet("diagnostics")]
    public async Task<ActionResult<SourceDiagnosticsReport>> GetDiagnostics()
    {
        var report = await _diagnosticsService.BuildReportAsync();
        return Ok(report);
    }

    [HttpGet("events")]
    public async Task<ActionResult<List<NewsEvent>>> GetEvents()
    {
        var allArticles = new List<SourceArticle>();

        foreach (var source in _sources)
        {
            try
            {
                var articles = await source.GetArticlesAsync();
                allArticles.AddRange(articles);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Source {SourceType} failed while loading events.", source.GetType().Name);
            }
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