using System.Globalization;
using System.Text;
using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public class RssEventMapper
{
    private readonly CzechLocationCatalog _locations;

    public RssEventMapper(CzechLocationCatalog locations)
    {
        _locations = locations;
    }

    public NewsEvent? MapToEvent(RssArticle article)
    {
        var title = article.Title ?? "";
        var summary = article.Summary ?? "";
        var fullText = title + " " + summary;

        var location = _locations.Find(title) ?? _locations.Find(summary);
        if (location == null)
        {
            return null;
        }

        return new NewsEvent
        {
            Title = title,
            SourceName = article.SourceName ?? "",
            SourceUrl = article.Link ?? "",
            LocationName = location.Name,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            EventType = GetEventType(fullText),
            Date = article.PublishedAt
        };
    }

    private string GetEventType(string text)
    {
        var normalized = Normalize(text);

        if (ContainsAny(normalized, "policie", "policista", "policisté", "gibs", "zásah", "zákrok", "zákroku", "krádež", "loupež", "napaden", "zadrž", "zatkl", "soud", "vězení", "zločin", "pátrá", "pachatel", "podezřel", "zbraň", "zbraní", "ozbrojen", "kufr", "zavazadl", "evakuovali"))
            return "crime";

        if (ContainsAny(normalized, "film", "festival", "divadlo", "divadle", "divadla", "koncert", "výstava", "kultura", "hudba", "muzeum", "opera", "opery", "premiéra", "premiér", "herec", "herečka", "hollywood", "hoffman", "filharmonie", "symfonici", "akrobat", "gothic", "hra na hrdiny"))
            return "culture";

        if (ContainsAny(normalized, "umělá inteligence", "umela inteligence", "ai olymp", "technologie", "technolog", "věda", "výzkum", "výzkumn", "robot", "kyber", "digital", "aplikace", "software"))
            return "technology";

        if (ContainsAny(normalized, "nemocnice", "lékař", "zdravot", "pacient", "epidemie", "vakcína", "léčba", "senior", "domov pro seniory", "sociální služ", "péče"))
            return "health";

        if (ContainsAny(normalized, "firma", "ekonomika", "podnik", "koruna", "ceny", "inflace", "tržby", "akcie", "bank", "pokuta", "sleva", "slevy", "obchod", "řetězec", "market", "penny"))
            return "business";

        if (ContainsAny(normalized, "mrakodrap", "budova", "stavba", "architekt", "developer", "bydlení", "rekonstrukce", "náměstí", "veřejný prostor"))
            return "urban";

        if (ContainsAny(normalized, "vláda", "prezident", "ministr", "sněmovna", "senát", "volby", "koalice", "opozice", "primátor", "starosta"))
            return "politics";

        if (ContainsAny(normalized, "škola", "univerzita", "student", "maturita", "učitel", "školství", "olympiáda", "olympiády"))
            return "education";

        if (ContainsAny(normalized, "hokej", "fotbal", "tenis", "sport", "liga", "utkání", "zápas", "kometa", "sparta", "slavia", "olympiáda", "olympiádu"))
            return "sport";

        if (ContainsAny(normalized, "požár", "hoří", "hořel", "hořela", "zapálil", "hasiči", "výbuch", "exploze"))
            return "fire";

        if (ContainsAny(normalized, "nehoda", "srážka", "bouračka", "dálnice", "silnice", "kamion", "dopravu", "zastavili dopravu", "provoz", "kolona", "d1", "d2", "d5", "d8", "d11"))
            return "traffic";

        if (ContainsAny(normalized, "tramvaj", "metro", "autobus", "vlak", "mhd", "letiště", "železnice", "dopravní podnik"))
            return "transport";

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
}