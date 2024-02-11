using CosmicTalent.Shared.Models;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Services;
using Microsoft.Extensions.Logging;

namespace CosmicTalent.Shared.Repositories
{
    public class MessageRepository : MongoDbRepository<Message>, IMessageRepository
    {
        public MessageRepository(MongoDbContext context, ILogger<MessageRepository> logger) : base(context.Messages, logger)
        {
        }
        public async Task InsertMessageAsync(Message message)
        {
            await InsertAsync(message);
        }
        public async Task<List<Message>> GetMessagesBySessionIdAsync(string sessionId)
        {
            return await FindByFilterAsync(x => x.Type == nameof(Message) && x.SessionId == sessionId);
        }
    }
}
