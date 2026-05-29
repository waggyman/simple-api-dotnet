using Microsoft.AspNetCore.Mvc;
using simple_api.Application.Contracts;
using simple_api.Application.Dtos;

namespace simple_api.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Conflict(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(result.Value);
    }
}
