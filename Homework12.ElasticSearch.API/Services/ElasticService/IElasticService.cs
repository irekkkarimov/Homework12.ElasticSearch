using Homework12.ElasticSearch.API.Models;

namespace Homework12.ElasticSearch.API.Services.ElasticService;

public interface IElasticService<T>
{
    Task<bool> AddOrUpdateAsync(T entity);
    Task<bool> AddOrUpdateBulkAsync(IEnumerable<T> entity);
    Task<bool> DeleteAsync(string key);
    Task<T?> GetAsync(string key);
    Task<IReadOnlyCollection<T>> SearchAsync(string searchString, string fieldName);
    Task<double> GetAvgId();
}