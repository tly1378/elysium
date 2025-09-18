using Microsoft.AspNetCore.Mvc;

namespace Elysium.Models.World;

[ApiController]
[Route("[controller]")]
public class WorldController(WorldService worldService, ILogger<WorldController> logger) : ControllerBase
{
    /// <summary>
    /// 生成一个新的世界地图
    /// </summary>
    /// <param name="worldId">世界ID</param>
    /// <param name="worldTheme">世界主题描述</param>
    /// <returns>World对象</returns>
    [HttpPost("generate-map")]
    public async Task<IActionResult> GenerateWorldMap([FromQuery] string worldId, [FromQuery] string worldTheme)
    {
        if (string.IsNullOrEmpty(worldId) || string.IsNullOrEmpty(worldTheme))
        {
            return BadRequest("WorldId and WorldTheme are required");
        }
        
        try
        {
            World? world = await worldService.GenerateWorldMapAsync(worldId, worldTheme);
            
            if (world == null)
            {
                return StatusCode(500, "Failed to generate world map");
            }
            
            return Ok(world);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating world map for {worldId}", worldId);
            return StatusCode(500, "An error occurred while generating the world map");
        }
    }
    
    /// <summary>
    /// 获取所有世界
    /// </summary>
    /// <returns>世界列表</returns>
    [HttpGet("all-worlds")]
    public IActionResult GetAllWorlds()
    {
        try
        {
            var worlds = worldService.GetAllWorlds();
            return Ok(worlds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all worlds");
            return StatusCode(500, "An error occurred while getting all worlds");
        }
    }
    
    /// <summary>
    /// 获取指定ID的世界
    /// </summary>
    /// <param name="worldId">世界ID</param>
    /// <returns>世界对象</returns>
    [HttpGet("{worldId}")]
    public IActionResult GetWorld(string worldId)
    {
        if (string.IsNullOrEmpty(worldId))
        {
            return BadRequest("WorldId is required");
        }
        
        try
        {
            if (worldService.TryGetWorld(worldId, out var world))
            {
                return Ok(world);
            }
            
            return NotFound($"World with ID '{worldId}' not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting world with ID {worldId}", worldId);
            return StatusCode(500, "An error occurred while getting the world");
        }
    }
}