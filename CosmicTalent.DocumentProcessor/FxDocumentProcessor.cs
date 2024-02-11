using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Configuration;
using AzureFunctions.Extensions.Middleware;
using AzureFunctions.Extensions.Middleware.Abstractions;
using CosmicTalent.DocumentProcessor.Interfaces;
using System.Collections.Generic;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using System.Linq;

namespace CosmicTalent.DocumentProcessor
{
    public class FxDocumentProcessor
    {
        private readonly IHttpMiddlewareBuilder _middlewareBuilder;
        private readonly ILogger<FxDocumentProcessor> _logger;
        private readonly IDocumentRecognizerService _documentRecognizerClient;
        private readonly CloudBlobClient _blobClient;
        private readonly string _containerName;
        public FxDocumentProcessor(ILogger<FxDocumentProcessor> log, CloudBlobClient blobClient, IConfiguration configuration, IHttpMiddlewareBuilder httpMiddlewareBuilder, IDocumentRecognizerService documentRecognizerService)
        {
            _logger = log;
            _blobClient = blobClient;
            _containerName = configuration["BlobContainerName"];
            _middlewareBuilder = httpMiddlewareBuilder;
            _documentRecognizerClient = documentRecognizerService;
        }

        [FunctionName("FxDocumentProcessor")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [OpenApiOperation(operationId: "ProcessResume", tags: new[] { "Resume process" })]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(MultiPartFormDataModel), Required = true, Description = "Resume data")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "File uploaded successfully.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "No file found in the request or invalid file format.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "An error occurred.")]
        public async Task<IActionResult> ProcessResume(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cosmictalent/resume")] HttpRequest req,
        [ServiceBus("%ServiceBusQueueName%", Connection = "ServiceBusConnString", EntityType = ServiceBusEntityType.Queue)] IAsyncCollector<CosmicTalentResume> serviceBus,
        ILogger log, ExecutionContext executionContext)
        {
            return await _middlewareBuilder.ExecuteAsync(new HttpMiddleware(async (httpContext) =>
            {
                _logger.LogInformation("C# HTTP trigger function received a request.");

                try
                {
                    var file = req.Form.Files[0];

                    if (file == null)
                    {
                        _logger.LogError("No file found in the request.");
                        return new BadRequestObjectResult("No file found in the request.");
                    }

                    var fileName = file.FileName;
                    var fileExtension = Path.GetExtension(fileName);

                    if (fileExtension != ".pdf" && fileExtension != ".docx")
                    {
                        _logger.LogError("Invalid file format. Only .pdf and .docx files are allowed.");
                        return new BadRequestObjectResult("Invalid file format. Only .pdf and .docx files are allowed.");
                    }

                    var blobName = $"{Guid.NewGuid()}{fileExtension}";

                    using var uploadStream = new MemoryStream();
                    using var documentStream = new MemoryStream();

                    await file.CopyToAsync(uploadStream);
                    await file.CopyToAsync(documentStream);

                    uploadStream.Position = 0;
                    documentStream.Position = 0;

                    string blobUri = string.Empty;

                    var uploadResumeTask = Task.Run(async () =>
                    {
                        CloudBlobContainer container = _blobClient.GetContainerReference(_containerName);
                        await container.CreateIfNotExistsAsync();
                        CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                        await blob.UploadFromStreamAsync(uploadStream);
                        Uri blobUri = blob.Uri;
                        return blobUri.ToString();
                    }).ContinueWith(s=> blobUri = s.Result);

                    AnalyzeResult analysedResult = default;

                    var resumeRecognizerTask = _documentRecognizerClient.GetDocumentDetails(documentStream, "ResumeModelId").ContinueWith(s => analysedResult = s.Result);

                    await Task.WhenAll(new List<Task>() { uploadResumeTask, resumeRecognizerTask });

                    await serviceBus.AddAsync(new CosmicTalentResume { BlobUri = blobUri, Content = analysedResult.Content, Documents = analysedResult.Documents.ToList(), Tables= analysedResult.Tables.ToList() });

                    _logger.LogInformation($"File uploaded to blob container with name: {blobName}");

                    return new OkObjectResult($"File uploaded to blob container with name: {blobName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }, executionContext));
        }
    }
    public class MultiPartFormDataModel
    {
        public byte[] FileUpload { get; set; }
    }
    public class CosmicTalentResume
    {
        public string BlobUri { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string Content { get; set; }
        public List<AnalyzedDocument> Documents { get; set; } = default;
        public List<DocumentTable> Tables { get; set; } = default;  
    }
}

