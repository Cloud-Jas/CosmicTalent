using CosmicTalent.Shared.Models;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Services;
using Microsoft.Extensions.Logging;

namespace CosmicTalent.Shared.Repositories
{
    public class SessionRepository : MongoDbRepository<Session>, ISessionRepository
    {
        public SessionRepository(MongoDbContext context, ILogger<SessionRepository> logger) : base(context.Sessions, logger)
        {

        }
        public async Task<List<Session>> GetSessionsAsync()
        {
            return await FindByFilterAsync(x => x.Type == nameof(Session));
        }
        public async Task CreateSessionAsync()
        {                   
            await InsertAsync(new Session());
        }
        public async Task ReplaceSessionAsync(Session session)
        {
            await UpdateAsync(session);
        }
        public async Task DeleteSessionAsync(string sessionId)
        {
            await DeleteAsync(x => x.Id == sessionId);
        }
    }
}
