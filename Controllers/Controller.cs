using Microsoft.AspNetCore.Mvc;
using BIA601.Services;

[ApiController]
[Route("api/recommend")]
public class RecommendationController : ControllerBase
{
    private readonly EvolutionEngine engine;

    public RecommendationController()
    {
        
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrEmpty(connStr))
        {
            throw new Exception("Connection string is NULL ❌");
        }

        engine = new EvolutionEngine(connStr);
    }

    [HttpGet("{userId}")]
    public IActionResult Get(int userId)
    {
        var result = engine.GetRecommendations(userId);
        return Ok(result);
    }
}
