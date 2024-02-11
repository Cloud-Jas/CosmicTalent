using CosmicTalent.Shared.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace CosmicTalent.Shared.Repositories
{
    public class MongoDbRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;
        private readonly ILogger<MongoDbRepository<T>> _logger;
        public MongoDbRepository(IMongoCollection<T> collection, ILogger<MongoDbRepository<T>> logger)
        {
            _logger = logger;
            _collection = collection;
        }
        public bool CheckIfVectorIndexExists(string indexName)
        {
            try
            {
                using IAsyncCursor<BsonDocument> cursor = _collection.Indexes.List();
                bool isVectorIndexExists = cursor.ToList().Exists(x => x["name"] == indexName);

                return isVectorIndexExists;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception message: {ex.Message} Stack trace : {ex.StackTrace}");
                throw;
            }
        }
        public async Task<string> VectorSearchAsync(float[] embeddings, string vectorPathProperty, int searchResultCount = 10)
        {

            string resultDocuments = string.Empty;

            try
            {                
                var embeddingsArray = new BsonArray(embeddings.Select(e => new BsonDouble(Convert.ToDouble(e))));
                BsonDocument[] pipeline =
                    [BsonDocument.Parse($"{{$search: {{cosmosSearch: {{ vector: [{string.Join(',', embeddings)}], path: '{vectorPathProperty}', k: {searchResultCount}}}, returnStoredSource:true}}}}"),
                    BsonDocument.Parse($"{{$project: {{{vectorPathProperty}: 0}}}}")];

                List<BsonDocument> bsonDocuments = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
                List<string> result = bsonDocuments.ConvertAll(bsonDocument => bsonDocument.ToString());
                resultDocuments = string.Join(" ", result);

            }
            catch (MongoException ex)
            {
                _logger.LogError($"Exception message: {ex.Message} Stack trace : {ex.StackTrace}");
                throw;
            }

            return resultDocuments;
        }
        public void CreateVectorIndex(string indexName, string collectionName, string vectorPathProperty, string kind = "vector-hnsw", int efConstructions = 64, int dimensions = 1536, int maxConnections = 16, string similarity = "COS")
        {
            try
            {
                BsonDocumentCommand<BsonDocument> command;
                if (kind.Equals("vector-hnsw"))
                {
                    command = new BsonDocumentCommand<BsonDocument>(
                        BsonDocument.Parse($@"
                            {{ createIndexes: '{collectionName}', 
                              indexes: [{{ 
                                name: '{indexName}', 
                                key: {{ {vectorPathProperty}: 'cosmosSearch' }}, 
                                cosmosSearchOptions: {{ kind: '{kind}', m: {maxConnections}, efConstruction: {efConstructions}, similarity: '{similarity}', dimensions: {dimensions} }} 
                              }}] 
                            }}"));
                }
                else
                {
                    command = new BsonDocumentCommand<BsonDocument>(
                        BsonDocument.Parse($@"
                            {{ createIndexes: '{collectionName}', 
                              indexes: [{{ 
                                name: '{indexName}', 
                                key: {{ {vectorPathProperty}: 'cosmosSearch' }}, 
                                cosmosSearchOptions: {{ kind: '{kind}', numLists: 5, similarity: '{similarity}', dimensions: {dimensions} }} 
                              }}] 
                            }}"));
                }

                BsonDocument result = _collection.Database.RunCommand(command);
                if (result["ok"] != 1)
                {
                    _logger.LogError("CreateVectorIndex failed " + result.ToJson());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception message: {ex.Message} Stack trace : {ex.StackTrace}");
                throw;
            }

        }
        public async Task InsertAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            await _collection.ReplaceOneAsync(
                filter: Builders<T>.Filter.Eq("_id", entity.Id),
                options: new ReplaceOptions { IsUpsert = true },
                replacement: entity);
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
        public async Task<List<T>> FindByFilterAsync(Expression<Func<T, bool>> filter)
        {           
            return (await _collection.FindAsync(filter)).ToList();
        }
        public async Task<T> GetById(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return (await _collection.FindAsync(filter)).FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            return _collection.Find(_ => true).ToList();
        }
    }
}
