using Azure.AI.OpenAI;
using Azure;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;

//string oaiEndpoint = config["AzureOAIEndpoint"] ?? "";
//string oaiKey = config["AzureOAIKey"] ?? "";
//string oaiDeploymentName = config["AzureOAIDeploymentName"] ?? "";
//string azureSearchEndpoint = config["AzureSearchEndpoint"] ?? "";
//string azureSearchKey = config["AzureSearchKey"] ?? "";
//string azureSearchIndex = config["AzureSearchIndex"] ?? "";

string oaiEndpoint = "https://labsselfserviceai.openai.azure.com";
string oaiKey = "aa3aeb60dcba4494801671e28b516302";
string oaiDeploymentName =  "";
string azureSearchEndpoint =  "";
string azureSearchKey = "";
string azureSearchIndex = "";

// Initialize the Azure OpenAI client
OpenAIClient client = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey));

// Create extension config for own data
AzureCognitiveSearchChatExtensionConfiguration ownDataConfig = new()
{
    SearchEndpoint = new Uri(azureSearchEndpoint),
    IndexName = azureSearchIndex
};
ownDataConfig.SetSearchKey(azureSearchKey);


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    //var forecast =  Enumerable.Range(1, 5).Select(index =>
    //    new WeatherForecast
    //    (
    //        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    //        Random.Shared.Next(-20, 55),
    //        summaries[Random.Shared.Next(summaries.Length)]
    //    ))
    //    .ToArray();
    //return forecast;

    ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
    {
        Messages =
    {
        new ChatMessage(ChatRole.User, "what is the weather in London")
    },
        MaxTokens = 600,
        Temperature = 0.9f,
        DeploymentName = oaiDeploymentName,
        // Specify extension options
        AzureExtensionsOptions = new AzureChatExtensionsOptions()
        {
            Extensions = { ownDataConfig }
        }
    };

    ChatCompletions response = client.GetChatCompletions(chatCompletionsOptions);
    ChatMessage responseMessage = response.Choices[0].Message;

    foreach (ChatMessage contextMessage in responseMessage.AzureExtensionsContext.Messages)
    {
        string contextContent = contextMessage.Content;
        try
        {
            var contextMessageJson = JsonDocument.Parse(contextMessage.Content);
            contextContent = JsonSerializer.Serialize(contextMessageJson, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
        }
        catch (JsonException)
        { }
        Console.WriteLine($"{contextMessage.Role}: {contextContent}");
    }
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
