
using Azure;
using Azure.AI.OpenAI;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Repositories;
using CosmicTalent.Shared.Services;

namespace CosmicTalent.WorkerService
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var openAIEndpoint = builder.Configuration.GetValue<string>("OpenAI:Endpoint");
            var openAIKey = builder.Configuration.GetValue<string>("OpenAI:ApiKey");
            builder.AddServiceDefaults();
            builder.Services.AddHostedService<Worker>();
            var mongoDbConnString = builder.Configuration.GetValue<string>("MongoDb:ConnectionString");
            var mongoDbName = builder.Configuration.GetValue<string>("MongoDb:DatabaseName");
            builder.Services.AddTransient(sp => new MongoDbContext(mongoDbConnString ?? throw new ArgumentNullException(mongoDbConnString),
                mongoDbName ?? throw new ArgumentNullException(mongoDbName)));
            builder.Services.AddTransient<OpenAIClient>(sp =>
            {
                var endpoint = new Uri(openAIEndpoint ?? throw new ArgumentNullException(openAIEndpoint));
                var credential = new AzureKeyCredential(openAIKey ?? throw new ArgumentNullException(openAIKey));
                var openAIClient = new OpenAIClient(endpoint, credential);

                return openAIClient;
            });
            builder.Services.AddTransient<IOpenAIService, OpenAIService>();
            builder.Services.AddTransient<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddTransient<IResumeEmbeddingRepository, ResumeEmbeddingRepository>();
            builder.AddAzureServiceBus("ServiceBusConnection");
            var host = builder.Build();
            host.Run();
        }
    }
}