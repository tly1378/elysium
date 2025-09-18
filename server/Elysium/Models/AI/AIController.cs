using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Elysium.Models.AI;

[ApiController]
[Route("[controller]")]
public class AIController(AIService aiService) : ControllerBase
{
    private static readonly List<Message> messages = [];
    
    [HttpPost("ask")]
    public async Task<ActionResult> Ask(string question, bool? isThinking = null)
    {
        messages.Add(new Message("user", [new Content("text", question)]));
        JObject? answer = await aiService.Fetch(messages.ToArray(), isThinking);
        if (answer == null) return BadRequest();
        var content = AIService.GetContent(answer);
        messages.Add(new Message("assistant", [new Content("text", content)]));
        UsageData usage = aiService.GetUsage(answer);
        return Content(content + "\n\n" + usage);
    }
}