using CosmicTalent.Shared.Models;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace CosmicTalent.Shared.Repositories
{
    public class ResumeEmbeddingRepository : MongoDbRepository<ResumeEmbedding>, IResumeEmbeddingRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string EmployeeResumeVectorProperty;
        public ResumeEmbeddingRepository(MongoDbContext context, ILogger<ResumeEmbeddingRepository> logger, IConfiguration configuration) : base(context.ResumeEmbeddings, logger)
        {
            _configuration = configuration;
            EmployeeResumeVectorProperty = _configuration.GetValue<string>("Employee:ResumeVector") ?? throw new ArgumentNullException(nameof(EmployeeResumeVectorProperty));
        }
        public async Task UpsertResumeEmbeddingAsync(ResumeEmbedding resumeEmbedding)
        {
            var emp = (await FindByFilterAsync(x => x.EmpId == resumeEmbedding.EmpId && x.Type == resumeEmbedding.Type)).FirstOrDefault();

            if (emp is null)
            {
                await InsertAsync(resumeEmbedding);
            }
            else
            {
                resumeEmbedding.Id = emp.Id;
                await UpdateAsync(resumeEmbedding);
            }
        }
        public async Task<string> EmployeeProfileVectorSearchAsync(float[] embeddings, int searchResult = 10)
        {
            return await VectorSearchAsync(embeddings, EmployeeResumeVectorProperty, searchResult);
        }
    }
}
