using Elysium.Models.Agent;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Elysium.Models.World;

public class WorldService(AgentService agentService, ILogger<WorldService> logger)
{
    private static readonly Dictionary<string, World> worlds = [];

    private readonly IDeserializer yamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    
    /// <summary>
    /// 生成一个新的世界地图描述（无向图）
    /// </summary>
    /// <param name="worldId">世界ID</param>
    /// <param name="worldTheme">世界主题描述</param>
    /// <returns>World对象</returns>
    public async Task<World?> GenerateWorldMapAsync(string worldId, string worldTheme)
    {
        // 检查世界是否已存在
        if (worlds.ContainsKey(worldId))
        {
            logger.LogError("World {worldId} already exists!", worldId);
            return null;
        }
        
        // 创建或获取用于生成世界的Agent
        var agent = agentService.NewAgent($"world_generator_{worldId}") ?? 
                   (agentService.TryGetAgent($"world_generator_{worldId}", out var existingAgent) ? existingAgent : null);
        
        if (agent == null)
        {
            logger.LogError("Failed to create or get world generator agent");
            return null;
        }
        
        // 构建提示词，要求Agent生成无向图格式的世界地图描述
        string prompt = $"""
        请为我生成一个主题为'{worldTheme}'的世界地图描述，使用YAML格式输出，格式如下：
        
        locations:
          - name: 地点名称1
            description: 地点描述1
          - name: 地点名称2
            description: 地点描述2
        connections:
          - from: 地点名称1
            to: 地点名称2
          - from: 地点名称2
            to: 地点名称3

        请确保输出是纯粹的YAML格式，不要包含任何额外的解释文字。
        """;

        // 调用AgentService的对话功能
        var response = await agentService.AskAgent(agent.Value, prompt);
        
        if (response == null)
        {
            logger.LogError("Failed to get response from world generator agent");
            return null;
        }
        
        // 获取Agent的回答内容（YAML格式的世界地图描述）
        string yamlWorldMap = response.content[0].text;
        
        try
        {
            // 反序列化YAML到临时对象结构
            var yamlData = yamlDeserializer.Deserialize<YamlWorldMap>(yamlWorldMap);
            
            // 创建World对象并映射数据
            var world = new World
            {
                Rooms = yamlData.Locations?.Select(loc => new Room
                {
                    Name = loc.Name,
                    Description = loc.Description
                }).ToList() ?? [],
                Connections = yamlData.Connections ?? []
            };
            
            // 保存世界对象
            worlds[worldId] = world;
            
            logger.LogInformation("Successfully generated world map for {worldId}", worldId);
            return world;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize world map for {worldId}", worldId);
            return null;
        }
    }
    
    /// <summary>
    /// 获取所有世界
    /// </summary>
    /// <returns>世界数组</returns>
    public World[] GetAllWorlds() => worlds.Values.ToArray();
    
    /// <summary>
    /// 尝试获取指定ID的世界
    /// </summary>
    /// <param name="id">世界ID</param>
    /// <param name="world">世界对象</param>
    /// <returns>是否成功获取</returns>
    public bool TryGetWorld(string id, out World world) => worlds.TryGetValue(id, out world);
}

public class World
{
    public List<Room> Rooms { get; set; } = [];
    public List<Connection> Connections { get; set; } = [];
}

public class Room
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class Connection
{
    public string From { get; set; }
    public string To { get; set; }
}

class YamlWorldMap
{
    public List<YamlLocation> Locations { get; set; }
    public List<Connection> Connections { get; set; }
}

class YamlLocation
{
    public string Name { get; set; }
    public string Description { get; set; }
}
