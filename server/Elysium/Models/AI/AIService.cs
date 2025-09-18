using System.Text;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Elysium.Models.AI;

public class AIService(ILogger<AIService> logger, HttpClient httpClient)
{
    private const string API_URL = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";
    private const string MODEL = "doubao-seed-1-6-250615";

    public async Task<JObject?> Fetch(Message[] messages, bool? isThinking = null)
    {
        string thinking = "auto";
        if (isThinking.HasValue)
        {
            thinking = isThinking.Value ? "enabled" : "disabled";
        }

        var requestBody = new
        {
            model = MODEL,
            messages,
            thinking = new
            {
                type = thinking
            }
        };
        
        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync(API_URL, httpContent);
        string responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("请求失败[{statusCode}]: {responseBody}", response.StatusCode, responseBody);
            return null;
        }

        logger.LogInformation("API 响应: {responseBody}", responseBody);
        return JObject.Parse(responseBody);
    }

    public static string GetContent(JObject json)
    {
        var firstChoice = (JObject)json["choices"][0];
        var message = (JObject)firstChoice["message"];
        return message["content"].ToString();
    }

    public UsageData GetUsage(JObject json)
    {
        var usage = json["usage"].ToObject<UsageData>();
        return usage;
    }
}

public record Message(string role, Content[] content, UsageData? usage = null);

public record Content(string type, string text);