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

        if (ContainsAny(normalized, "policie", "policista", "policist", "gibs", "zásah", "zákrok", "krádež", "loupež", "napaden", "zadrž", "zatkl", "vězení", "zločin", "pátrá", "pachatel", "podezřel", "zbraň", "ozbrojen", "kufr", "zavazadl", "evaku", "vražd", "ubodal", "narkoman", "trestn", "detenc", "obžal"))
            return "crime";

        if (ContainsAny(normalized, "film", "festival", "divadlo", "koncert", "výstav", "kultur", "hudb", "muze", "opera", "premiér", "herec", "herečk", "hollywood", "hoffman", "filharmon", "symfon", "akrobat", "gothic", "hra na hrdiny", "glóbus", "kameraman", "karlovar"))
            return "culture";

        if (ContainsAny(normalized, "hokej", "fotbal", "fotbalist", "tenis", "tenist", "sport", "liga", "utkání", "zápas", "kometa", "sparta", "slavia", "olympiád", "vondrouš", "muchov", "trenér"))
            return "sport";

        if (ContainsAny(normalized, "turist", "cestov", "hrad", "zámek", "památk", "zoo", "klementin", "čističk", "výlet", "návštěvn"))
            return "tourism";

        if (ContainsAny(normalized, "umělá inteligence", "umela inteligence", "ai olymp", "technolog", "věda", "výzkum", "robot", "kyber", "digital", "aplikace", "software"))
            return "technology";

        if (ContainsAny(normalized, "ekologi", "klima", "emis", "elektrár", "energet", "větrn", "přírod", "životní prostředí", "uhlí", "solár"))
            return "environment";

        if (ContainsAny(normalized, "počasí", "bouř", "vítr", "déšť", "sníh", "povodeň", "vedr", "mráz", "tropick", "meteorolog"))
            return "weather";

        if (ContainsAny(normalized, "nemocnic", "lékař", "zdravot", "pacient", "epidemi", "vakcín", "léčb", "senior", "domov pro seniory", "sociální služ", "péč"))
            return "health";


        if (ContainsAny(normalized, "firma", "ekonom", "podnik", "koruna", "ceny", "inflac", "tržb", "akci", "bank", "pokut", "slev", "obchod", "řetězec", "market", "penny", "miliard", "náhradu škody", "investic"))
            return "business";

        if (ContainsAny(normalized, "mrakodrap", "budov", "stavb", "architekt", "developer", "bydlen", "rekonstrukc", "náměst", "veřejný prostor"))
            return "urban";

        if (ContainsAny(normalized, "vlád", "prezident", "ministr", "sněmovn", "senát", "volb", "koalic", "opozic", "primátor", "starost", "poslanc", "zákon", "kompetenční žalob", "financování čt", "čro", "babiš", "pavel"))
            return "politics";

        if (ContainsAny(normalized, "škol", "univerzit", "student", "maturit", "učitel", "školstv", "deváťák", "přijat"))
            return "education";

        if (ContainsAny(normalized, "požár", "hoří", "hořel", "hořela", "zapálil", "hasič", "výbuch", "exploz"))
            return "fire";

        if (ContainsAny(normalized, "nehod", "srážk", "bouračk", "dálnic", "silnic", "kamion", "dopravu", "zastavili dopravu", "provoz", "kolon", "d1", "d2", "d5", "d8", "d11"))
            return "traffic";

        if (ContainsAny(normalized, "tramvaj", "metro", "autobus", "vlak", "mhd", "letišt", "železnic", "dopravní podnik"))
            return "transport";

        return "general";
    }

    private static bool ContainsAny(string normalizedText, params string[] patterns)
    {
        return patterns.Any(pattern => ContainsNormalized(normalizedText, pattern));
    }

    private static bool ContainsNormalized(string normalizedText, string pattern)
    {
        var normalizedPattern = Normalize(pattern).Trim();
        if (string.IsNullOrWhiteSpace(normalizedPattern))
        {
            return false;
        }

        if (normalizedPattern.Contains(' '))
        {
            return normalizedText.Contains($" {normalizedPattern} ");
        }

        return normalizedText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(word => word.StartsWith(normalizedPattern, StringComparison.Ordinal));
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