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
}