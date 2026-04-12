using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public class EventService
{
    public List<NewsEvent> GetEvents()
    {
        var now = DateTime.UtcNow;

        return new List<NewsEvent>
        {
            new NewsEvent
            {
                Title = "Napadení nožem v Praze",
                SourceName = "Novinky.cz",
                SourceUrl = "https://example.com",
                Latitude = 50.0755,
                Longitude = 14.4378,
                EventType = "stabbing",
                Date = now
            },
            new NewsEvent
            {
                Title = "Rvačka v centru Prahy",
                SourceName = "iDNES",
                SourceUrl = "https://example.com",
                Latitude = 50.0760,
                Longitude = 14.4385,
                EventType = "fight",
                Date = now.AddDays(-1)
            },
            new NewsEvent
            {
                Title = "Policie zatkla podezřelého",
                SourceName = "CNN Prima",
                SourceUrl = "https://example.com",
                Latitude = 50.0748,
                Longitude = 14.4369,
                EventType = "arrest",
                Date = now.AddDays(-3)
            },
            new NewsEvent
            {
                Title = "Loupež v obchodě",
                SourceName = "Deník",
                SourceUrl = "https://example.com",
                Latitude = 50.0759,
                Longitude = 14.4391,
                EventType = "robbery",
                Date = now.AddDays(-10)
            },
            new NewsEvent
            {
                Title = "Požár v bytě",
                SourceName = "Aktuálně",
                SourceUrl = "https://example.com",
                Latitude = 50.0763,
                Longitude = 14.4372,
                EventType = "fire",
                Date = now.AddDays(-2)
            },
            new NewsEvent
            {
                Title = "Zatčení v Brně",
                SourceName = "iDNES",
                SourceUrl = "https://example.com",
                Latitude = 49.1951,
                Longitude = 16.6068,
                EventType = "arrest",
                Date = now
            },
            new NewsEvent
            {
                Title = "Napadení na ulici",
                SourceName = "Novinky.cz",
                SourceUrl = "https://example.com",
                Latitude = 49.1955,
                Longitude = 16.6072,
                EventType = "fight",
                Date = now.AddDays(-4)
            },
            new NewsEvent
            {
                Title = "Střelba v Brně",
                SourceName = "CNN Prima",
                SourceUrl = "https://example.com",
                Latitude = 49.1948,
                Longitude = 16.6061,
                EventType = "shooting",
                Date = now.AddDays(-6)
            },
            new NewsEvent
            {
                Title = "Další rvačka",
                SourceName = "Deník",
                SourceUrl = "https://example.com",
                Latitude = 49.1959,
                Longitude = 16.6077,
                EventType = "fight",
                Date = now.AddDays(-8)
            },
            new NewsEvent
            {
                Title = "Policie zasáhla",
                SourceName = "Aktuálně",
                SourceUrl = "https://example.com",
                Latitude = 49.1945,
                Longitude = 16.6058,
                EventType = "arrest",
                Date = now.AddDays(-1)
            },
            new NewsEvent
            {
                Title = "Incident v Ostravě",
                SourceName = "Novinky.cz",
                SourceUrl = "https://example.com",
                Latitude = 49.8209,
                Longitude = 18.2625,
                EventType = "unknown",
                Date = now.AddDays(-15)
            },
            new NewsEvent
            {
                Title = "Událost v Plzni",
                SourceName = "iDNES",
                SourceUrl = "https://example.com",
                Latitude = 49.7384,
                Longitude = 13.3736,
                EventType = "unknown",
                Date = now.AddDays(-20)
            }
        };
    }
}