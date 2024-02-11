using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Models;
using CosmicTalent.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CosmicTalent.TalentProcessor.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IResumeEmbeddingRepository _resumeEmbeddingRepository;

        public ChatController(IMessageRepository messageRepository, ISessionRepository sessionRepository, IEmployeeRepository employeeRepository, IResumeEmbeddingRepository resumeEmbeddingRepository)
        {
            _messageRepository = messageRepository;
            _sessionRepository = sessionRepository;
            _employeeRepository = employeeRepository;
            _resumeEmbeddingRepository = resumeEmbeddingRepository;
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<List<Session>>> GetAllChatSessionsAsync()
        {
            try
            {
                var sessions = await _sessionRepository.GetSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Session>>> GetAllChatSessionsTestAsync()
        {
            try
            {
                var sessions = await _sessionRepository.GetSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("messages/{sessionId}")]
        public async Task<ActionResult<List<Message>>> GetChatSessionMessagesAsync(string sessionId)
        {
            try
            {
                var chatMessages = await _messageRepository.GetMessagesBySessionIdAsync(sessionId);
                return Ok(chatMessages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("sessions")]
        public async Task<ActionResult> CreateNewChatSessionAsync()
        {
            try
            {
                await _sessionRepository.CreateSessionAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<ActionResult> AddNewChatMessageAsync(Message message)
        {
            try
            {
                await _messageRepository.InsertMessageAsync(message);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("employee/vectorSearch")]
        public async Task<ActionResult> VectorSearchAsync(float[] embeddings)
        {
            try
            {
                var response = await _resumeEmbeddingRepository.EmployeeProfileVectorSearchAsync(embeddings);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("sessions/{sessionId}")]
        public async Task<ActionResult<string>> RenameSessionAsync(Session session,string sessionId)
        {
            try
            {
                await _sessionRepository.ReplaceSessionAsync(session);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("sessions/{sessionId}")]
        public async Task<ActionResult<string>> DeleteSessionAsync(string sessionId)
        {
            try
            {
                await _sessionRepository.DeleteSessionAsync(sessionId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}