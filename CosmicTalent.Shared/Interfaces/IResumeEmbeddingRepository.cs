using CosmicTalent.Shared.Models;

namespace CosmicTalent.Shared.Interfaces
{
    public interface IResumeEmbeddingRepository : IRepository<ResumeEmbedding>
    {
        public Task<string> EmployeeProfileVectorSearchAsync(float[] embeddings, int searchResult = 10);
        public Task UpsertResumeEmbeddingAsync(ResumeEmbedding resumeEmbedding);
    }
}
