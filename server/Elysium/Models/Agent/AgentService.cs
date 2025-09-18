using Elysium.Models.AI;
using Newtonsoft.Json.Linq;

namespace Elysium.Models.Agent;

public class AgentService(AIService aiService, ILogger<AgentService> logger)
{
    private static readonly Dictionary<string, Agent> agents = [];
    
    public async Task<Message?> AskAgent(Agent agent, string question, bool? isThinking = null)
    {
        agent.Messages.Add(new Message("user", [new Content("text", question)]));
        JObject? answer = await aiService.Fetch(agent.Messages.ToArray(), isThinking);
        if (answer == null)
        {
            logger.LogError("AgentService::AskAgent returned null");
            return null;
        }
        
        var content = AIService.GetContent(answer);
        UsageData usage = aiService.GetUsage(answer);
        var message = new Message("assistant", [new Content("text", content)], usage);
        agent.Messages.Add(message);
        return message;
    }
    
    public Agent[] GetAllAgents() => agents.Values.ToArray();

    public bool TryGetAgent(string id, out Agent agent) => agents.TryGetValue(id, out agent);
    
    public Agent? NewAgent(string id)
    {
        if(agents.ContainsKey(id))
        {
            logger.LogError("Agent {id} already exists!", id);
            return null;
        }
        
        var agent = new Agent(id);
        agents.Add(id, agent);
        return agent;
    }
}