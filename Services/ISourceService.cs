using CzechNewsMap.Api.Models;

namespace CzechNewsMap.Api.Services;

public interface ISourceService
{
    Task<List<SourceArticle>> GetArticlesAsync();
}