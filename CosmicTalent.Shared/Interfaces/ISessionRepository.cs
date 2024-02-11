using CosmicTalent.Shared.Models;

namespace CosmicTalent.Shared.Interfaces
{
    public interface ISessionRepository
    {
        public Task<List<Session>> GetSessionsAsync();
        public Task CreateSessionAsync();
        public Task ReplaceSessionAsync(Session session);
        public Task DeleteSessionAsync(string sessionId);
    }
}