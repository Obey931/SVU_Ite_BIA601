using Microsoft.AspNetCore.Mvc;
using BIA601.Services;

[ApiController]
[Route("api/recommend")]
public class RecommendationController : ControllerBase
{
    private readonly EvolutionEngine engine;

    public RecommendationController()
    {
        string connStr = "server=localhost;port=3306;user=root;password=;database=ecommerce;";
        engine = new EvolutionEngine(connStr);
    }

    [HttpGet("{userId}")]
    public IActionResult Get(int userId)
    {
        var result = engine.GetRecommendations(userId);
        return Ok(result);
    }
}