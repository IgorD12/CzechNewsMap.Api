namespace CzechNewsMap.Api.Models;

public class SourceDiagnosticsReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int TotalArticles { get; set; }
    public int DeduplicatedArticles { get; set; }
    public int GeolocatedArticles { get; set; }
    public int RejectedArticles { get; set; }
    public double GeolocationRate { get; set; }
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
    public List<SourceDiagnosticsItem> Sources { get; set; } = new();
    public List<DiagnosticsArticleSample> GeneralSamples { get; set; } = new();
    public List<DiagnosticsArticleSample> RejectedSamples { get; set; } = new();
}

public class SourceDiagnosticsItem
{
    public string SourceName { get; set; } = "";
    public int FetchedArticles { get; set; }
    public int GeolocatedArticles { get; set; }
    public int RejectedArticles { get; set; }
    public double GeolocationRate { get; set; }
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
    public List<DiagnosticsArticleSample> RejectedSamples { get; set; } = new();
    public string? Error { get; set; }
}

public class DiagnosticsArticleSample
{
    public string Title { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string? LocationName { get; set; }
    public string? EventType { get; set; }
    public string Link { get; set; } = "";
}