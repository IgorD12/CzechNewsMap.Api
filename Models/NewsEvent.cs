namespace CzechNewsMap.Api.Models;

public class NewsEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public DateTime PublishedAt { get; set; }
    public string EventType { get; set; } = "";
    public string LocationName { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationPrecision { get; set; } = "";
    public int UncertaintyRadiusMeters { get; set; }
}