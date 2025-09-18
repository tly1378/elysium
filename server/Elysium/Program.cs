using System.Net.Http.Headers;
using Elysium.Models.Agent;
using Elysium.Models.AI;
using Elysium.Models.World;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<AIService>();
builder.Services.AddSingleton<AgentService>();
builder.Services.AddSingleton<WorldService>();
builder.Services.AddHttpClient<AIService>(client => 
{
    const string apiKey = "9c54d975-1b27-4a46-976e-4c6404b37c4b";
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

WebApplication app = builder.Build();
app.MapGet("/", () => "Running...");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();