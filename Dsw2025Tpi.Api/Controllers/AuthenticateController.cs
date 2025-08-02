using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("login")] // admin: Gabriel GabrielMoeykens7# / user:FranciscoVicente FranciscoVicente1.
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

        var token = _jwtTokenService.GenerateToken(user);
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

        await _userManager.AddToRoleAsync(user, "User");
        return Ok("Usuario Registrado con Exito");

    }

    // hago un endpoint para probar el tema de los roles
    [Authorize(Roles = "Admin")]
    [HttpGet("solo-admin")]
    public IActionResult AdminEndpoint()
    {
        return Ok(" Hola Admin :D ");
    }

    [Authorize(Roles = "User")]
    [HttpGet("solo-user")]
    public IActionResult UserEndpoint()
    {
        return Ok(" Hola User :D ");
    }

}

