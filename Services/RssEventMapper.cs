using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public class RssEventMapper
{
    public NewsEvent? MapToEvent(RssArticle article)
    {
        var title = (article.Title ?? "").ToLower();
        var summary = (article.Summary ?? "").ToLower();
        var fullText = title + " " + summary;

        if (!IsIncidentNews(fullText))
            return null;

        var location = GetLocation(title) ?? GetLocation(summary);
        if (location == null)
            return null;

        var eventType = GetEventType(fullText);

        return new NewsEvent
        {
            Title = article.Title ?? "",
            SourceName = article.SourceName ?? "",
            SourceUrl = article.Link ?? "",
            Latitude = location.Value.lat,
            Longitude = location.Value.lng,
            EventType = eventType,
            Date = article.PublishedAt
    };
    }

    private bool IsIncidentNews(string text)
{
    return ContainsAny(text,
        "policie",
        "napaden",
        "rvačka",
        "střelba",
        "požár",
        "nehoda",
        "zraněn",
        "zranila",
        "zranil",
        "zabila",
        "zabil",
        "mrtvý",
        "mrtvá",
        "krádež",
        "loupež",
        "výbuch",
        "hasiči",
        "zásah",
        "srazil",
        "srážka",
        "útok",
        "násilí",
        "zločin",
        "demolice",
        "most",
        "uzavírky",
        "omezení"
    );
}

    private (double lat, double lng)? GetLocation(string text)
    {
        // Praha
        if (ContainsAny(text, " praha ", " praze ", " prahy ", "praha,", "praze,", "prahy,"))
            return (50.0755, 14.4378);

        // Brno
        if (ContainsAny(text, " brno ", " brně ", " brna ", "brno,", "brně,", "brna,"))
            return (49.1951, 16.6068);

        // Ostrava
        if (ContainsAny(text, " ostrava ", " ostravě ", " ostravy ", "ostrava,", "ostravě,", "ostravy,"))
            return (49.8209, 18.2625);

        // Plzeň
        if (ContainsAny(text, " plzeň ", " plzni ", " plzen ", "plzeň,", "plzni,", "plzen,"))
            return (49.7384, 13.3736);

        // Liberec
        if (ContainsAny(text, " liberec ", " liberci ", "liberec,", "liberci,"))
            return (50.7671, 15.0562);

        // Jihlava
        if (ContainsAny(text, " jihlava ", " jihlavě ", " jihlavy ", "jihlava,", "jihlavě,", "jihlavy,"))
            return (49.3961, 15.5912);

        // Hodkovice nad Mohelkou
        if (ContainsAny(text, " hodkovice ", " hodkovicích ", "hodkovice,", "hodkovicích,"))
            return (50.6659, 15.0898);

        return null;
    }

    private string GetEventType(string text)
    {
        if (ContainsAny(text, "nůž", "nožem", "pobod", "bodl", "bodná"))
            return "stabbing";

        if (ContainsAny(text, "střelba", "střílel", "výstřel", "zbraň", "zbraní"))
            return "shooting";

        if (ContainsAny(text, "zatkl", "zadrž", "dopadla policie", "dopadli", "zásah policie"))
            return "arrest";

        if (ContainsAny(text, "rvačka", "napaden", "napadl", "napadla", "potyčka"))
            return "fight";

        if (ContainsAny(text, "loupež", "oloupil", "krádež", "ukradl"))
            return "robbery";

        if (ContainsAny(text, "požár", "hořel", "hořela", "hoří", "zapálil", "hasiči"))
            return "fire";

        return "unknown";
    }

    private bool ContainsAny(string text, params string[] patterns)
    {
        var normalized = " " + text + " ";
        return patterns.Any(p => normalized.Contains(p));
    }
}