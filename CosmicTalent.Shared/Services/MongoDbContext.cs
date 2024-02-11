using CosmicTalent.Shared.Models;
using MongoDB.Driver;

namespace CosmicTalent.Shared.Services
{
    public class MongoDbContext
    {        
        private readonly IMongoDatabase _database;                
        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);            
        }
        public IMongoCollection<Employee> Employees => _database.GetCollection<Employee>("employees");
        public IMongoCollection<Message> Messages => _database.GetCollection<Message>("chats");
        public IMongoCollection<Session> Sessions => _database.GetCollection<Session>("chats");
        public IMongoCollection<ResumeEmbedding> ResumeEmbeddings => _database.GetCollection<ResumeEmbedding>("resumeembeddings");
    }
}
