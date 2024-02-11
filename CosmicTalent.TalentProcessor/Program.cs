
using Azure.AI.OpenAI;
using Azure;
using CosmicTalent.Shared.Services;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var openAIEndpoint = builder.Configuration.GetValue<string>("OpenAI:Endpoint");
var openAIKey = builder.Configuration.GetValue<string>("OpenAI:ApiKey");
var mongoDbConnString = builder.Configuration.GetValue<string>("MongoDb:ConnectionString");
var mongoDbName = builder.Configuration.GetValue<string>("MongoDb:DatabaseName");
builder.Services.AddSingleton(sp => new MongoDbContext(mongoDbConnString ?? throw new ArgumentNullException(mongoDbConnString),
    mongoDbName ?? throw new ArgumentNullException(mongoDbName)));

builder.Services.AddSingleton(sp =>
{
    var endpoint = new Uri(openAIEndpoint ?? throw new ArgumentNullException(openAIEndpoint));
    var credential = new AzureKeyCredential(openAIKey ?? throw new ArgumentNullException(openAIKey));
    var openAIClient = new OpenAIClient(endpoint, credential);

    return openAIClient;
});
builder.Services.AddTransient<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddTransient<IMessageRepository, MessageRepository>();
builder.Services.AddTransient<ISessionRepository, SessionRepository>();
builder.Services.AddTransient<IResumeEmbeddingRepository, ResumeEmbeddingRepository>();
builder.Services.AddTransient<IOpenAIService, OpenAIService>();
var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();
