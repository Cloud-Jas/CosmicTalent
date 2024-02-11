using CosmicTalent.ChatApp;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddRazorPages();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddServerSideBlazor();

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ChatApiClient>();
builder.Services.AddHttpClient("MongoDbClient", client => {
    client.BaseAddress = new("http://talentprocessor/api/chat/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("OpenAIClient", client => {
    client.BaseAddress = new("http://talentprocessor/api/openai/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseStaticFiles();

app.UseAntiforgery();

app.UseOutputCache();

app.MapDefaultEndpoints();

app.Run();
