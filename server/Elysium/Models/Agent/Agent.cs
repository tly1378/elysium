using Elysium.Models.AI;

namespace Elysium.Models.Agent;

public readonly struct Agent(string id)
{
    public string Id { get; } = id;
    public List<Message> Messages { get; } = [];
}