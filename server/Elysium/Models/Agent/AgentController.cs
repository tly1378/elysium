using Microsoft.AspNetCore.Mvc;

namespace Elysium.Models.Agent;

[ApiController]
[Route("[controller]")]
public class AgentController(ILogger<AgentController> logger): ControllerBase
{
    private static readonly List<Agent> agents = [];
    
    [HttpGet("agents")]
    public ActionResult GetAgents()
    {
        logger.Log(LogLevel.Information, "Getting all agents");
        return Ok(agents);
    }
    
    [HttpGet("new")]
    public ActionResult NewAgent(string name)
    {
        logger.Log(LogLevel.Information, "Creating new agent");
        agents.Add(new Agent(name));
        return Ok();
    }

    private readonly struct Agent(string name)
    {
        public string Name { get; } = name;
    }
}