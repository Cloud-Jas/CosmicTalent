using CosmicTalent.Shared.Models;
using MongoDB.Bson;

namespace CosmicTalent.Shared.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {        
        public Task UpsertEmployeeAsync(Employee employee);
        public Task<string> EmployeeProfileVectorSearchAsync(float[] embeddings, int searchResult = 10);
    }
}