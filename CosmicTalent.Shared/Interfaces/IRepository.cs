using MongoDB.Bson;
using System.Linq.Expressions;

public interface IRepository<T>
{
    public Task InsertAsync(T entity);
    public Task UpdateAsync(T entity);
    public Task DeleteAsync(Expression<Func<T, bool>> filter);
    public bool CheckIfVectorIndexExists(string indexName);
    public void CreateVectorIndex(string indexName, string collectionName, string vectorPathProperty, string kind = "vector-hnsw", int efConstructions = 64, int dimensions = 1536, int maxConnections = 16, string similarity = "COS");
    public Task <T> GetById(string id);
    public IEnumerable<T> GetAll();
}
