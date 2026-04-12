using Microsoft.AspNetCore.Mvc;
using CzechNewsMap.Api.Services;

namespace CzechNewsMap.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly EventService _eventService;

    public EventsController(EventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var events = _eventService.GetEvents();
        return Ok(events);
    }

    [HttpGet("from-rss")]
    public async Task<IActionResult> GetFromRss(
        [FromServices] RssService rssService,
        [FromServices] RssEventMapper mapper)
    {
        var articles = await rssService.GetLatestArticlesAsync(30);

        var events = articles
            .Select(a => mapper.MapToEvent(a))
            .Where(e => e != null)
            .ToList();

        return Ok(events);
    }
}

