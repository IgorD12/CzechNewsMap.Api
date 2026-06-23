using System.Globalization;
using System.Text;
using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public class RssEventMapper
{
    private static readonly LocationRule[] LocationRules =
    {
        new("Praha", 50.0755, 14.4378, "praha", "praze", "prahy", "prahou", "pražský"),
        new("Brno", 49.1951, 16.6068, "brno", "brně", "brna", "brnem", "brněnský"),
        new("Ostrava", 49.8209, 18.2625, "ostrava", "ostravě", "ostravy", "ostravský"),
        new("Plzeň", 49.7384, 13.3736, "plzeň", "plzni", "plzen", "plzeňský"),
        new("Liberec", 50.7671, 15.0562, "liberec", "liberci", "liberecký"),
        new("Olomouc", 49.5938, 17.2509, "olomouc", "olomouci", "olomoucký"),
        new("České Budějovice", 48.9745, 14.4743, "české budějovice", "českých budějovicích", "budějovice", "budějovicích"),
        new("Hradec Králové", 50.2104, 15.8252, "hradec králové", "hradci králové", "královéhradecký"),
        new("Pardubice", 50.0343, 15.7812, "pardubice", "pardubicích", "pardubický"),
        new("Ústí nad Labem", 50.6607, 14.0323, "ústí nad labem", "ústecký"),
        new("Karlovy Vary", 50.2319, 12.8710, "karlovy vary", "karlových varech", "karlovarský"),
        new("Zlín", 49.2244, 17.6628, "zlín", "zlíně", "zlínský"),
        new("Jihlava", 49.3961, 15.5912, "jihlava", "jihlavě", "jihlavy", "vysočina"),
        new("Kladno", 50.1431, 14.1052, "kladno", "kladně", "kladenska"),
        new("Mladá Boleslav", 50.4114, 14.9032, "mladá boleslav", "mladé boleslavi", "boleslav"),
        new("Most", 50.5030, 13.6362, "mostecko", "mostecku", "okres most"),
        new("Opava", 49.9387, 17.9026, "opava", "opavě", "opavsko"),
        new("Karviná", 49.8567, 18.5432, "karviná", "karviné", "karvinsko"),
        new("Havířov", 49.7800, 18.4369, "havířov", "havířově"),
        new("Frýdek-Místek", 49.6819, 18.3673, "frýdek místek", "frýdku místku", "frýdecko místecko"),
        new("Hodkovice nad Mohelkou", 50.6659, 15.0898, "hodkovice", "hodkovicích", "hodkovice nad mohelkou")
    };

    public NewsEvent? MapToEvent(RssArticle article)
    {
        var title = article.Title ?? "";
        var summary = article.Summary ?? "";
        var fullText = title + " " + summary;

        var location = GetLocation(fullText);
        if (location == null)
        {
            return null;
        }

        return new NewsEvent
        {
            Title = title,
            SourceName = article.SourceName ?? "",
            SourceUrl = article.Link ?? "",
            Latitude = location.Value.lat,
            Longitude = location.Value.lng,
            EventType = GetEventType(fullText),
            Date = article.PublishedAt
        };
    }

    private (double lat, double lng)? GetLocation(string text)
    {
        var normalized = Normalize(text);

        foreach (var location in LocationRules)
        {
            if (location.Patterns.Any(pattern => ContainsNormalized(normalized, pattern)))
            {
                return (location.Latitude, location.Longitude);
            }
        }

        return null;
    }

    private string GetEventType(string text)
    {
        var normalized = Normalize(text);

        if (ContainsAny(normalized, "hokej", "fotbal", "tenis", "sport", "liga", "utkání", "zápas", "kometa", "sparta", "slavia", "olympiáda", "olympiádu"))
            return "sport";

        if (ContainsAny(normalized, "film", "festival", "divadlo", "divadle", "divadla", "koncert", "výstava", "kultura", "hudba", "muzeum", "opera", "opery", "premiéra", "premiér"))
            return "culture";

        if (ContainsAny(normalized, "požár", "hoří", "hořel", "hořela", "zapálil", "hasiči", "výbuch", "exploze"))
            return "fire";

        if (ContainsAny(normalized, "nehoda", "srážka", "bouračka", "dálnice", "silnice", "vlak narazil", "auto", "kamion", "dopravu", "zastavili dopravu"))
            return "traffic";

        if (ContainsAny(normalized, "tramvaj", "metro", "autobus", "vlak", "mhd", "letiště", "železnice", "dopravní podnik"))
            return "transport";

        if (ContainsAny(normalized, "policie", "policista", "policisté", "gibs", "zákrok", "zákroku", "krádež", "loupež", "napaden", "zadrž", "zatkl", "soud", "vězení", "zločin", "pátrá", "pachatel"))
            return "crime";

        if (ContainsAny(normalized, "vláda", "prezident", "ministr", "sněmovna", "senát", "volby", "koalice", "opozice", "primátor", "starosta"))
            return "politics";

        if (ContainsAny(normalized, "nemocnice", "lékař", "zdravot", "pacient", "epidemie", "vakcína", "léčba"))
            return "health";

        if (ContainsAny(normalized, "škola", "univerzita", "student", "maturita", "učitel", "školství"))
            return "education";

        if (ContainsAny(normalized, "firma", "ekonomika", "podnik", "koruna", "ceny", "inflace", "tržby", "akcie", "bank"))
            return "business";

        if (ContainsAny(normalized, "počasí", "bouřka", "vítr", "déšť", "sníh", "povodeň", "vedro", "mráz"))
            return "weather";

        return "general";
    }

    private static bool ContainsAny(string normalizedText, params string[] patterns)
    {
        return patterns.Any(pattern => ContainsNormalized(normalizedText, pattern));
    }

    private static bool ContainsNormalized(string normalizedText, string pattern)
    {
        return normalizedText.Contains(Normalize(pattern));
    }

    private static string Normalize(string text)
    {
        var decomposed = (text ?? "").ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length + 2);
        builder.Append(' ');

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }

        builder.Append(' ');
        return string.Join(" ", builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Insert(0, " ") + " ";
    }

    private sealed record LocationRule(string Name, double Latitude, double Longitude, params string[] Patterns);
}