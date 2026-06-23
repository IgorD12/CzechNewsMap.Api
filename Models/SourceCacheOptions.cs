namespace CzechNewsMap.Api.Models;

public class SourceCacheOptions
{
    public int DurationMinutes { get; set; } = 10;
    public int BrowserCacheSeconds { get; set; } = 60;
}