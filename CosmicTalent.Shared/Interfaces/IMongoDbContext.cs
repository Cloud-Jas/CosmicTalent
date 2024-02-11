namespace CosmicTalent.Shared.Interfaces
{
    public interface IMongoDbContext
    {
        public void CreateVectorIndex(string indexName, string collectionName);
        public bool CheckIfVectorIndexExists(string indexName,string collectionName);
    }
}
