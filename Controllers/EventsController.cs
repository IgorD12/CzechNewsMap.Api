using Microsoft.AspNetCore.Mvc;

namespace CzechNewsMap.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var now = DateTime.UtcNow;

        var events = new[]
        {
            // Прага (кластер)
            new {
                title = "Napadení nožem v Praze",
                sourceName = "Novinky.cz",
                sourceUrl = "https://example.com",
                latitude = 50.0755,
                longitude = 14.4378,
                eventType = "stabbing",
                date = now
            },
            new {
                title = "Rvačka v centru Prahy",
                sourceName = "iDNES",
                sourceUrl = "https://example.com",
                latitude = 50.0760,
                longitude = 14.4385,
                eventType = "fight",
                date = now.AddDays(-1)
            },
            new {
                title = "Policie zatkla podezřelého",
                sourceName = "CNN Prima",
                sourceUrl = "https://example.com",
                latitude = 50.0748,
                longitude = 14.4369,
                eventType = "arrest",
                date = now.AddDays(-3)
            },
            new {
                title = "Loupež v obchodě",
                sourceName = "Deník",
                sourceUrl = "https://example.com",
                latitude = 50.0759,
                longitude = 14.4391,
                eventType = "robbery",
                date = now.AddDays(-10)
            },
            new {
                title = "Požár v bytě",
                sourceName = "Aktuálně",
                sourceUrl = "https://example.com",
                latitude = 50.0763,
                longitude = 14.4372,
                eventType = "fire",
                date = now.AddDays(-2)
            },

            // Брно (кластер)
            new {
                title = "Zatčení v Brně",
                sourceName = "iDNES",
                sourceUrl = "https://example.com",
                latitude = 49.1951,
                longitude = 16.6068,
                eventType = "arrest",
                date = now
            },
            new {
                title = "Napadení na ulici",
                sourceName = "Novinky.cz",
                sourceUrl = "https://example.com",
                latitude = 49.1955,
                longitude = 16.6072,
                eventType = "fight",
                date = now.AddDays(-4)
            },
            new {
                title = "Střelba v Brně",
                sourceName = "CNN Prima",
                sourceUrl = "https://example.com",
                latitude = 49.1948,
                longitude = 16.6061,
                eventType = "shooting",
                date = now.AddDays(-6)
            },
            new {
                title = "Další rvačka",
                sourceName = "Deník",
                sourceUrl = "https://example.com",
                latitude = 49.1959,
                longitude = 16.6077,
                eventType = "fight",
                date = now.AddDays(-8)
            },
            new {
                title = "Policie zasáhla",
                sourceName = "Aktuálně",
                sourceUrl = "https://example.com",
                latitude = 49.1945,
                longitude = 16.6058,
                eventType = "arrest",
                date = now.AddDays(-1)
            },

            // одиночные точки
            new {
                title = "Incident v Ostravě",
                sourceName = "Novinky.cz",
                sourceUrl = "https://example.com",
                latitude = 49.8209,
                longitude = 18.2625,
                eventType = "unknown",
                date = now.AddDays(-15)
            },
            new {
                title = "Událost v Plzni",
                sourceName = "iDNES",
                sourceUrl = "https://example.com",
                latitude = 49.7384,
                longitude = 13.3736,
                eventType = "unknown",
                date = now.AddDays(-20)
            }
        };

        return Ok(events);
    }
}