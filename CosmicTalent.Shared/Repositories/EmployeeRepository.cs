using CosmicTalent.Shared.Models;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace CosmicTalent.Shared.Repositories
{
    public class EmployeeRepository : MongoDbRepository<Employee>, IEmployeeRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string EmployeeResumeVectorProperty;
        public EmployeeRepository(MongoDbContext context, ILogger<EmployeeRepository> logger, IConfiguration configuration) : base(context.Employees, logger)
        {
            _configuration = configuration;
            EmployeeResumeVectorProperty = _configuration.GetValue<string>("Employee:ResumeVector") ?? throw new ArgumentNullException(nameof(EmployeeResumeVectorProperty));
        }

        public async Task UpsertEmployeeAsync(Employee employee)
        {
            var emp = (await FindByFilterAsync(x => x.EmpId == employee.EmpId)).FirstOrDefault();

            if (emp is null)
            {
                await InsertAsync(employee);
            }
            else
            {
                employee.Id = emp.Id;
                await UpdateAsync(employee);
            }
        }
        public async Task<string> EmployeeProfileVectorSearchAsync(float[] embeddings, int searchResult = 10)
        {
            return await VectorSearchAsync(embeddings, EmployeeResumeVectorProperty, searchResult);
        }
    }
}
