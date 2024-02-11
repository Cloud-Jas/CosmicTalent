using MongoDB.Bson.Serialization.Attributes;

namespace CosmicTalent.Shared.Models
{
    public class Chat : BaseEntity
    {      
        public string Type { get; set; }
        public int? TokensUsed { get; set; }
        public string SessionId { get; set; }
    }
    public class Session : Chat
    {                
        public string Name { get; set; }
        [BsonIgnore]
        public List<Message>? Messages { get; set; }

        public Session()
        {           
            Type = nameof(Session);
            SessionId = Id;
            TokensUsed = 0;
            Name = "New Chat";
            Messages = new List<Message>();
        }

        public void AddMessage(Message message)
        {
            Messages?.Add(message);
        }

        public void UpdateMessage(Message message)
        {
            if (Messages != null)
            {
                var match = Messages.Single(m => m.Id == message.Id);
                var index = Messages.IndexOf(match);
                Messages[index] = message;
            }
        }
    }
    public class Message : Chat
    {       
        public Message() { }
        public DateTime TimeStamp { get; set; }
        public string Sender { get; set; }
        public int Tokens { get; set; }
        public int PromptTokens { get; set; }
        public string Text { get; set; }
        public Message(string sessionId, string sender, int? tokens, int? promptTokens, string text)
        {         
            Type = nameof(Message);
            SessionId = sessionId;
            Sender = sender;
            Tokens = tokens ?? 0;
            PromptTokens = promptTokens ?? 0;
            TimeStamp = DateTime.UtcNow;
            Text = text;
        }
    }
}