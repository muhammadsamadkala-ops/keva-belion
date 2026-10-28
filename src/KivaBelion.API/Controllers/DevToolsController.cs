using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace KivaBelion.API.Controllers;

/// <summary>
/// Practice endpoints that prove error handling and validation work.
/// They answer "404 Not Found" everywhere except on a developer PC (Development environment).
/// They are removed before launch.
/// </summary>
[ApiController]
[Route("api/v1/dev")]
public sealed class DevToolsController(IHostEnvironment environment) : ControllerBase
{
    /// <summary>Deliberately fails, to prove the global error handler returns a safe 500.</summary>
    [HttpGet("throw")]
    public IActionResult Throw()
    {
        if (!environment.IsDevelopment()) return NotFound();
        throw new InvalidOperationException("This is a deliberate test error.");
    }

    /// <summary>Accepts a small test form, to prove validation returns clear 400 errors.</summary>
    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ValidationDemoRequest request)
    {
        if (!environment.IsDevelopment()) return NotFound();
        return Ok(new { Message = "Valid request.", request.Name, request.Quantity });
    }
}

public sealed class ValidationDemoRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Quantity { get; set; }
}
