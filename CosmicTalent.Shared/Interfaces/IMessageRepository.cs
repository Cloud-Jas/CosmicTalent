using CosmicTalent.Shared.Models;

namespace CosmicTalent.Shared.Interfaces
{
    public interface IMessageRepository
    {
        public Task<List<Message>> GetMessagesBySessionIdAsync(string sessionId);
        public Task InsertMessageAsync(Message message);
    }
}