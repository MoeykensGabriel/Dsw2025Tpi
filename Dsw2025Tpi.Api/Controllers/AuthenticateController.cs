using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    private readonly JwtTokenService _jwtTokenService;

    public AuthenticateController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> identityUser,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = identityUser;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if(user == null)
        {
            return Unauthorized("Usuario o Contraseña Incorrectos");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password,false);
        if(!result.Succeeded)
        {
            return Unauthorized("Usuario o Contraseña Incorrectos");
        }

        var token = _jwtTokenService.GenerateToken(request.Username);
        return Ok(new {token});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Usuario Registrado con Exito");
    }

}

