using CzechNewsMap.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CzechNewsMap.Api.Controllers;

[ApiController]
[Route("api/rss")]
public class RssController : ControllerBase
{
    private readonly RssService _rssService;

    public RssController(RssService rssService)
    {
        _rssService = rssService;
    }

    [HttpGet("test")]
    public async Task<IActionResult> GetLatest([FromQuery] int maxItems = 10)
    {
        try
        {
            var articles = await _rssService.GetLatestArticlesAsync(maxItems);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Failed to load RSS feed.",
                error = ex.Message
            });
        }
    }
}