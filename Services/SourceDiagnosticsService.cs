using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public class SourceDiagnosticsService
{
    private const int MaxSamples = 8;

    private readonly IEnumerable<ISourceService> _sources;
    private readonly ArticleDedupService _dedupService;
    private readonly RssEventMapper _mapper;

    public SourceDiagnosticsService(
        IEnumerable<ISourceService> sources,
        ArticleDedupService dedupService,
        RssEventMapper mapper)
    {
        _sources = sources;
        _dedupService = dedupService;
        _mapper = mapper;
    }

    public async Task<SourceDiagnosticsReport> BuildReportAsync()
    {
        var report = new SourceDiagnosticsReport
        {
            GeneratedAt = DateTime.UtcNow
        };

        var allArticles = new List<SourceArticle>();

        foreach (var source in _sources)
        {
            var sourceReport = new SourceDiagnosticsItem
            {
                SourceName = source.GetType().Name
            };

            try
            {
                var articles = await source.GetArticlesAsync();
                allArticles.AddRange(articles);

                sourceReport.SourceName = articles.FirstOrDefault()?.SourceName ?? sourceReport.SourceName;
                sourceReport.FetchedArticles = articles.Count;

                foreach (var article in articles)
                {
                    var newsEvent = _mapper.MapToEvent(ToRssArticle(article));

                    if (newsEvent == null)
                    {
                        sourceReport.RejectedArticles++;
                        AddSample(sourceReport.RejectedSamples, article, null);
                        continue;
                    }

                    sourceReport.GeolocatedArticles++;
                    Increment(sourceReport.CategoryCounts, newsEvent.EventType);
                }
            }
            catch (Exception ex)
            {
                sourceReport.Error = ex.Message;
            }

            sourceReport.GeolocationRate = CalculateRate(sourceReport.GeolocatedArticles, sourceReport.FetchedArticles);
            report.Sources.Add(sourceReport);
        }

        var deduplicated = _dedupService.Deduplicate(allArticles);
        report.TotalArticles = allArticles.Count;
        report.DeduplicatedArticles = deduplicated.Count;

        foreach (var article in deduplicated)
        {
            var newsEvent = _mapper.MapToEvent(ToRssArticle(article));

            if (newsEvent == null)
            {
                report.RejectedArticles++;
                AddSample(report.RejectedSamples, article, null);
                continue;
            }

            report.GeolocatedArticles++;
            Increment(report.CategoryCounts, newsEvent.EventType);

            if (newsEvent.EventType == "general")
            {
                AddSample(report.GeneralSamples, article, newsEvent);
            }
        }

        report.GeolocationRate = CalculateRate(report.GeolocatedArticles, report.DeduplicatedArticles);

        report.Sources = report.Sources
            .OrderByDescending(source => source.FetchedArticles)
            .ThenBy(source => source.SourceName)
            .ToList();

        report.CategoryCounts = OrderCounts(report.CategoryCounts);

        foreach (var source in report.Sources)
        {
            source.CategoryCounts = OrderCounts(source.CategoryCounts);
        }

        return report;
    }

    private static RssArticle ToRssArticle(SourceArticle article)
    {
        return new RssArticle
        {
            Title = article.Title,
            Link = article.Link,
            PublishedAt = article.PublishedAt,
            SourceName = article.SourceName,
            Summary = article.Summary
        };
    }

    private static void AddSample(List<DiagnosticsArticleSample> samples, SourceArticle article, NewsEvent? newsEvent)
    {
        if (samples.Count >= MaxSamples)
        {
            return;
        }

        samples.Add(new DiagnosticsArticleSample
        {
            Title = article.Title,
            SourceName = article.SourceName,
            Link = article.Link,
            LocationName = newsEvent?.LocationName,
            EventType = newsEvent?.EventType
        });
    }

    private static void Increment(Dictionary<string, int> counts, string key)
    {
        counts[key] = counts.GetValueOrDefault(key) + 1;
    }

    private static double CalculateRate(int count, int total)
    {
        return total == 0 ? 0 : Math.Round((double)count / total, 3);
    }

    private static Dictionary<string, int> OrderCounts(Dictionary<string, int> counts)
    {
        return counts
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}