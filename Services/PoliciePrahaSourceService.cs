using CzechNewsMap.Api.Models;
using HtmlAgilityPack;

namespace CzechNewsMap.Api.Services;

public class PoliciePrahaSourceService : ISourceService
{
    private readonly HttpClient _httpClient;

    private const string SourceUrl =
        "https://policie.gov.cz/sprava-hl-m-prahy-zpravodajstvi.aspx";

    public PoliciePrahaSourceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SourceArticle>> GetArticlesAsync()
    {
        var html = await _httpClient.GetStringAsync(SourceUrl);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var articles = new List<SourceArticle>();

        // Берем ссылки на články внутри основного контента
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");

        if (linkNodes == null)
            return articles;

        foreach (var linkNode in linkNodes)
        {
            var href = linkNode.GetAttributeValue("href", "").Trim();
            var title = HtmlEntity.DeEntitize(linkNode.InnerText).Trim();

            if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(title))
                continue;

            // Берем только police články
            if (!href.Contains("/clanek/"))
                continue;

            // Отсеиваем мусор
            if (title.Length < 8)
                continue;

            var absoluteUrl = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? href
                : $"https://policie.gov.cz{href}";

            articles.Add(new SourceArticle
            {
                Title = title,
                Link = absoluteUrl,
                PublishedAt = DateTime.UtcNow,
                SourceName = "Policie Praha",
                Summary = title
            });
        }

        // убираем дубли по ссылке внутри самого источника
        return articles
            .GroupBy(a => a.Link)
            .Select(g => g.First())
            .Take(30)
            .ToList();
    }
}