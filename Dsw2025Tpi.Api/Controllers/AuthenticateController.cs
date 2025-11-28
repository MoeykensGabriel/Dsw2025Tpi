using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IRepository _repository;

    public AuthenticateController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> identityUser,
        JwtTokenService jwtTokenService,
        IRepository repository
        )
    {
        _userManager = userManager;
        _signInManager = identityUser;
        _jwtTokenService = jwtTokenService;
        _repository = repository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return Unauthorized(new
                {
                    code = "INVALID_CREDENTIALS",
                    message = "Usuario o Contraseña Incorrectos"
                });
            }

            Console.WriteLine($"User found: {user.UserName}");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                Console.WriteLine("Password incorrect");
                return Unauthorized(new
                {
                    code = "INVALID_CREDENTIALS",
                    message = "Usuario o Contraseña Incorrectos"
                });
            }
            if (!Guid.TryParse(user.Id, out _))
            {
                user.Id = Guid.NewGuid().ToString();
                await _userManager.UpdateAsync(user);
            }
            Console.WriteLine("Generating token...");
            var token = await _jwtTokenService.GenerateToken(user);
            // obtener rol del usuario
            var roles = await _userManager.GetRolesAsync(user);
            var userResponse = new
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles  // roles en la respuesta
            };
            return Ok(new { token = token, user = userResponse });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");

            return StatusCode(500, new
            {
                code = "INTERNAL_ERROR",
                message = "Error interno del servidor"
            });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = model.Username,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join(" ", result.Errors.Select(e => e.Description));

            return BadRequest(new { message = errorMessages });
        }

        var roleToAdd = "User"; // Rol por defecto
        if (!string.IsNullOrEmpty(model.Role) && model.Role == "Admin")
        {
            roleToAdd = "Admin";
        }
        
        if (roleToAdd == "Admin")
        {
            await _userManager.RemoveFromRoleAsync(user, "User");
        }
        
        else
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        await _userManager.AddToRoleAsync(user, roleToAdd);
        return Ok("Usuario Registrado con Exito");
    }

    [HttpPost("register-customer")]
    [AllowAnonymous] 
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterModel model)
    {
        var newId = Guid.NewGuid().ToString();
        var user = new IdentityUser
        {
            Id = newId,
            UserName = model.Username,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = errorMessages });
        }

        // asignar ek rol
        await _userManager.AddToRoleAsync(user, "User");

        return Ok("Usuario Registrado con Exito");
    }

}

