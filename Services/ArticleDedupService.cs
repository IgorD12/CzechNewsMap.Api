using CzechNewsMap.Api.Models;
using System.Globalization;
using System.Text;

namespace CzechNewsMap.Api.Services;

public class ArticleDedupService
{
    public List<SourceArticle> Deduplicate(IEnumerable<SourceArticle> articles)
    {
        return articles
            .GroupBy(a => BuildKey(a))
            .Select(g => g
                .OrderByDescending(x => x.PublishedAt)
                .First())
            .ToList();
    }

    private string BuildKey(SourceArticle article)
    {
        var normalizedTitle = Normalize(article.Title);
        var day = article.PublishedAt.Date.ToString("yyyy-MM-dd");
        return $"{normalizedTitle}|{day}";
    }

    private string Normalize(string text)
    {
        text = text.ToLowerInvariant();

        var normalized = text.Normalize(NormalizationForm.FormD);

        var chars = normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        var noDiacritics = new string(chars).Normalize(NormalizationForm.FormC);

        var cleaned = new string(noDiacritics
            .Select(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ? c : ' ')
            .ToArray());

        return string.Join(" ", cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}