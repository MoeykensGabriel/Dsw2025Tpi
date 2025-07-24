using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;

    public AuthenticateController(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel request)
    {
        // validacion simple, aun sin bd
        if(request.Username == "admin" && request.Password == "password")
        {
            var token = _jwtTokenService.GenerateToken(request.Username, "tester");
            return Ok(new { token });
        }

        return Unauthorized();
    }

}

