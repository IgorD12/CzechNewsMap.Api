namespace CzechNewsMap.Api.Models;

public class SourceArticle
{
    public string Title { get; set; } = "";
    public string Link { get; set; } = "";
    public DateTime PublishedAt { get; set; }
    public string SourceName { get; set; } = "";
    public string Summary { get; set; } = "";
}