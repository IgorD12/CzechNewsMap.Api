namespace CzechNewsMap.Api.Models;

public class NewsEvent
{
    public string Title { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string EventType { get; set; } = "";
    public DateTime Date { get; set; }
}