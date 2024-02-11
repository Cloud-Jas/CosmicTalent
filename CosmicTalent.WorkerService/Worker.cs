using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Messaging.ServiceBus;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Models;
using Newtonsoft.Json;
using System.Text;

namespace CosmicTalent.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private readonly IOpenAIService _openAIService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IResumeEmbeddingRepository _resumeEmbeddingRepository;
        private readonly string EmployeeVectorSearchIndexName;
        private readonly string EmployeeCollectionName;
        private readonly string ResumeEmbeddingCollectionName;
        private readonly string EmployeeResumeVector;
        public Worker(ILogger<Worker> logger, ServiceBusClient serviceBusClient, IConfiguration configuration, IOpenAIService openAIService, IEmployeeRepository employeeRepository, IResumeEmbeddingRepository resumeEmbeddingRepository)
        {
            _logger = logger;
            var queueName = configuration.GetValue<string>("ServiceBusQueueName");
            _serviceBusProcessor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());
            _openAIService = openAIService;
            _employeeRepository = employeeRepository;
            EmployeeVectorSearchIndexName = configuration.GetValue<string>("Employee:VectorSearchIndexName");
            EmployeeCollectionName = configuration.GetValue<string>("Employee:CollectionName");
            ResumeEmbeddingCollectionName = configuration.GetValue<string>("ResumeEmbedding:CollectionName");
            EmployeeResumeVector = configuration.GetValue<string>("Employee:ResumeVector");
            _resumeEmbeddingRepository = resumeEmbeddingRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serviceBusProcessor.ProcessMessageAsync += ProcessMessagesAsync;

            _serviceBusProcessor.ProcessErrorAsync += ExceptionReceivedHandler;

            await _serviceBusProcessor.StartProcessingAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }

            await _serviceBusProcessor.StopProcessingAsync();
        }
        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var messageBody = Encoding.UTF8.GetString(args.Message.Body);
                var talentResume = JsonConvert.DeserializeObject<CosmicTalentResume>(messageBody);
                var chunks = new List<ResumeEmbedding>();

                if (talentResume != null && talentResume.Content != null && talentResume.Documents.Any())
                {
                    #region populate model with employeeid and name
                    foreach (CustomAnalyzedDocument document in talentResume.Documents)
                    {
                        talentResume.EmployeeId = document.Fields.EmployeeId.Content;
                        talentResume.EmployeeName = document.Fields.EmployeeName.Content;
                        chunks = await GetResumeEmbeddingChunks(document, talentResume.EmployeeId, talentResume.EmployeeName);
                    }
                    chunks.AddRange(await GetResumeEmbeddingChunks(talentResume.Tables, talentResume?.EmployeeId, talentResume?.EmployeeName));
                    #endregion

                    #region call openai service to generate embeddings
                    var (embeddings, tokens) = await _openAIService.GetEmbeddingsAsync(talentResume.Content);
                    #endregion

                    #region call mongo db service to create vector index if not exists        
                    var isVectorIndexExist = _employeeRepository.CheckIfVectorIndexExists(EmployeeVectorSearchIndexName);
                    if (!isVectorIndexExist)
                        _employeeRepository.CreateVectorIndex(EmployeeVectorSearchIndexName, EmployeeCollectionName, EmployeeResumeVector, "vector-ivf");
                    var isEmbeddingVectorIndexExist = _resumeEmbeddingRepository.CheckIfVectorIndexExists(EmployeeVectorSearchIndexName);
                    if (!isEmbeddingVectorIndexExist)
                        _resumeEmbeddingRepository.CreateVectorIndex(EmployeeVectorSearchIndexName, ResumeEmbeddingCollectionName, EmployeeResumeVector, "vector-ivf");
                    #endregion

                    #region call mongodb service to insert employee record along with embeddings
                    Employee employee = new Employee()
                    {
                        EmpId = talentResume.EmployeeId,
                        Name = talentResume.EmployeeName,
                        Content = talentResume.Content,
                        IsOccupied = false,
                        ResumeUri = talentResume.BlobUri,
                        ResumeVector = embeddings,
                        resumevector = embeddings
                    };
                    await _employeeRepository.UpsertEmployeeAsync(employee);
                    #endregion

                    var tasks = chunks.Select(async chunk =>
                    {
                        var resumeEmbedding = new ResumeEmbedding
                        {
                            Content = chunk.Content,
                            EmpId = chunk.EmpId,
                            EmpName = chunk.EmpName,
                            resumevector = chunk.resumevector,
                            Tokens = chunk.Tokens,
                            Type = chunk.Type
                        };
                        await _resumeEmbeddingRepository.UpsertResumeEmbeddingAsync(resumeEmbedding);
                    });

                    await Task.WhenAll(tasks);

                }
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing the message: {ex.Message}");
                await args.DeadLetterMessageAsync(args.Message);
            }
        }
        private Task ExceptionReceivedHandler(ProcessErrorEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            return Task.CompletedTask;
        }
        private async Task<List<ResumeEmbedding>> GetResumeEmbeddingChunks(List<CustomTable> tables, string? employeeId, string? employeeName)
        {
            var tasks = new List<Task<(string, float[], string)>>
            {
                GetResumeEmbeddingAsync(getConcatenatedSkillSets(tables), employeeId, employeeName, "Employee skill sets")
            };
            var results = await Task.WhenAll(tasks);

            return results.Select((result, index) => new ResumeEmbedding
            {
                EmpId = employeeId,
                EmpName = employeeName,
                Content = $"Provided information about {result.Item3} is for Employee Id {employeeId} and name {employeeName}: {result.Item1}",
                resumevector = result.Item2,
                Type = result.Item3
            }).ToList();
        }
        private async Task<List<ResumeEmbedding>> GetResumeEmbeddingChunks(CustomAnalyzedDocument document, string employeeId, string employeeName)
        {
            var tasks = new List<Task<(string, float[], string)>>
            {
                GetResumeEmbeddingAsync(document.Fields.EmployeeSummary.Content, employeeId, employeeName, "Employee Summary"),
                GetResumeEmbeddingAsync(document.Fields.EmployeeCertifications.Content, employeeId, employeeName, "Employee Certification"),
                GetResumeEmbeddingAsync(document.Fields.EmployeeAchievements.Content, employeeId, employeeName, "Employee Achievements"),
                GetResumeEmbeddingAsync(document.Fields.EmployeeExperienceHistory.Content, employeeId, employeeName, "Employee Experience History"),
                GetResumeEmbeddingAsync(document.Fields.EmployeeProject1.Content, employeeId, employeeName, "Previous worked projects-1"),
                GetResumeEmbeddingAsync(document.Fields.EmployeeProject2.Content, employeeId, employeeName, "Previous worked projects-2"),
                GetResumeEmbeddingAsync(document.Fields.EmployeeProject3.Content, employeeId, employeeName, "Previous worked projects-3"),
                GetResumeEmbeddingAsync(document.Fields.EmployeeProject4.Content, employeeId, employeeName, "Previous worked projects-4")
            };

            var results = await Task.WhenAll(tasks);

            return results.Select((result, index) => new ResumeEmbedding
            {
                EmpId = employeeId,
                EmpName = employeeName,
                Content = $"Provided information about {result.Item3} is for Employee Id {employeeId} and name {employeeName}: {result.Item1}",
                resumevector = result.Item2,
                Type = result.Item3
            }).ToList();
        }
        private string? getConcatenatedSkillSets(List<CustomTable> tables)
        {
            var concatenatedRows = tables.Select(table =>
            {
                StringBuilder rowContents = new StringBuilder();
                for (int i = 0; i < table.RowCount; i++)
                {
                    var rowCells = table.Cells.Where(cell => cell.RowIndex == i).OrderBy(cell => cell.ColumnIndex);
                    rowContents.AppendLine(string.Join(", ", rowCells.Select(cell => cell.Content)));
                }
                return rowContents;
            }).FirstOrDefault();

            return concatenatedRows?.ToString();
        }
        private async Task<(string, float[], string)> GetResumeEmbeddingAsync(string content, string EmployeeId, string EmployeeName, string type)
        {
            if (type.StartsWith("Previous worked projects"))
            {
                content = await SummarizeContentAsync(content);
            }

            var PreLoadContext = $"Provided information about {type} is for Employee Id {EmployeeId} and name {EmployeeName}: ";                        
            
            var embedding = await _openAIService.GetEmbeddingsAsync(PreLoadContext + content);
            
            return (content, embedding.Item1, type);
        }
        private async Task<string> SummarizeContentAsync(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;
            var response = await _openAIService.SummarizeAsync(null, content);
            return response;
        }
    }
}
