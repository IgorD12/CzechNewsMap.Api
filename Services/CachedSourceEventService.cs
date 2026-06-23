using CzechNewsMap.Api.Models;
using Microsoft.Extensions.Options;

namespace CzechNewsMap.Api.Services;

public class CachedSourceEventService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<SourceCacheOptions> _options;
    private readonly ILogger<CachedSourceEventService> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private SourceEventCacheSnapshot? _snapshot;

    public CachedSourceEventService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<SourceCacheOptions> options,
        ILogger<CachedSourceEventService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<List<NewsEvent>> GetEventsAsync(bool refresh = false, CancellationToken cancellationToken = default)
    {
        var snapshot = _snapshot;
        if (!refresh && IsFresh(snapshot))
        {
            return snapshot!.Events.ToList();
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            snapshot = _snapshot;
            if (!refresh && IsFresh(snapshot))
            {
                return snapshot!.Events.ToList();
            }

            var events = await LoadEventsAsync(cancellationToken);
            _snapshot = new SourceEventCacheSnapshot(DateTimeOffset.UtcNow, events);

            return events.ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException && _snapshot != null)
        {
            _logger.LogError(ex, "Failed to refresh news cache. Returning stale cache from {GeneratedAt}.", _snapshot.GeneratedAt);
            return _snapshot.Events.ToList();
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private bool IsFresh(SourceEventCacheSnapshot? snapshot)
    {
        if (snapshot == null)
        {
            return false;
        }

        return DateTimeOffset.UtcNow - snapshot.GeneratedAt < GetCacheDuration();
    }

    private TimeSpan GetCacheDuration()
    {
        var minutes = Math.Max(1, _options.CurrentValue.DurationMinutes);
        return TimeSpan.FromMinutes(minutes);
    }

    private async Task<List<NewsEvent>> LoadEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var sources = scope.ServiceProvider.GetRequiredService<IEnumerable<ISourceService>>().ToList();
        var dedupService = scope.ServiceProvider.GetRequiredService<ArticleDedupService>();
        var mapper = scope.ServiceProvider.GetRequiredService<RssEventMapper>();

        if (sources.Count == 0)
        {
            throw new InvalidOperationException("No news sources are registered.");
        }

        var allArticles = new List<SourceArticle>();
        var failedSources = 0;

        foreach (var source in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var articles = await source.GetArticlesAsync();
                allArticles.AddRange(articles);

                var sourceName = articles.FirstOrDefault()?.SourceName ?? source.GetType().Name;
                _logger.LogInformation("Loaded {ArticleCount} articles from {SourceName}.", articles.Count, sourceName);
            }
            catch (Exception ex)
            {
                failedSources++;
                _logger.LogWarning(ex, "Source {SourceType} failed while refreshing news cache.", source.GetType().Name);
            }
        }

        if (allArticles.Count == 0 && failedSources > 0)
        {
            throw new InvalidOperationException("All configured news sources failed while refreshing news cache.");
        }

        var deduplicated = dedupService.Deduplicate(allArticles);
        var events = deduplicated
            .Select(article => mapper.MapToEvent(new RssArticle
            {
                Title = article.Title,
                Link = article.Link,
                PublishedAt = article.PublishedAt,
                SourceName = article.SourceName,
                Summary = article.Summary
            }))
            .Where(newsEvent => newsEvent != null)
            .Cast<NewsEvent>()
            .ToList();

        _logger.LogInformation(
            "News cache refreshed with {EventCount} geolocated events from {ArticleCount} articles ({DeduplicatedCount} after deduplication).",
            events.Count,
            allArticles.Count,
            deduplicated.Count);

        return events;
    }

    private sealed record SourceEventCacheSnapshot(DateTimeOffset GeneratedAt, List<NewsEvent> Events);
}