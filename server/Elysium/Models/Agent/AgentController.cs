using Elysium.Models.AI;
using Microsoft.AspNetCore.Mvc;

namespace Elysium.Models.Agent;

[ApiController]
[Route("[controller]")]
public class AgentController(ILogger<AgentController> logger, AgentService agentService): ControllerBase
{
    [HttpGet("all-agents")]
    public ActionResult GetAgents()
    {
        logger.LogInformation("Getting all agents");
        return Ok(string.Join("\n", agentService.GetAllAgents()));
    }
    
    [HttpGet("{id}")]
    public ActionResult GetAgent(string id)
    {
        logger.LogInformation("Getting agent {name}", id);
        if (!agentService.TryGetAgent(id, out Agent agent))
        {
            return NotFound(id);
        }

        return Ok(agent);
    }
    
    [HttpPost("{id}/new")]
    public ActionResult NewAgent(string id)
    {
        logger.LogInformation("Creating new agent: {id}", id);
        if (agentService.TryGetAgent(id, out _))
        {
            return BadRequest("Agent already exists");
        }
        
        var agent = agentService.NewAgent(id);
        return Ok(agent);
    }

    [HttpPost("{id}/ask")]
    public async Task<ActionResult> AskAgent(string id, string message)
    {
        logger.LogInformation("Asking agent {name}: {message}", id, message);
        if (!agentService.TryGetAgent(id, out Agent agent))
        {
            return NotFound("Agent not found");
        }
        
        Message? answer = await agentService.AskAgent(agent, message);
        return Ok(answer);
    }
}