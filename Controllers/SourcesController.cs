using CzechNewsMap.Api.Models;
using CzechNewsMap.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CzechNewsMap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly CachedSourceEventService _eventCache;
    private readonly SourceDiagnosticsService _diagnosticsService;
    private readonly SourceCacheOptions _cacheOptions;

    public SourcesController(
        CachedSourceEventService eventCache,
        SourceDiagnosticsService diagnosticsService,
        IOptions<SourceCacheOptions> cacheOptions)
    {
        _eventCache = eventCache;
        _diagnosticsService = diagnosticsService;
        _cacheOptions = cacheOptions.Value;
    }

    [HttpGet("diagnostics")]
    public async Task<ActionResult<SourceDiagnosticsReport>> GetDiagnostics()
    {
        var report = await _diagnosticsService.BuildReportAsync();
        return Ok(report);
    }

    [HttpGet("events")]
    public async Task<ActionResult<List<NewsEvent>>> GetEvents([FromQuery] bool refresh = false)
    {
        var events = await _eventCache.GetEventsAsync(refresh, HttpContext.RequestAborted);
        var browserCacheSeconds = Math.Max(0, _cacheOptions.BrowserCacheSeconds);

        Response.Headers.CacheControl = refresh || browserCacheSeconds == 0
            ? "no-store"
            : $"public, max-age={browserCacheSeconds}";

        return Ok(events);
    }
}