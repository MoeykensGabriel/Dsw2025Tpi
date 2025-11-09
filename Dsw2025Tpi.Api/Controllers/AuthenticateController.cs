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

            Console.WriteLine("Generating token...");
            var token = await _jwtTokenService.GenerateToken(user);
            // Obtenemos los roles del usuario
            var roles = await _userManager.GetRolesAsync(user);
            var userResponse = new
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles  // Añadimos los roles a la respuesta
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
            UserName = model.Username,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            // 1. Unimos todos los errores (ya traducidos por SpanishIdentityErrorDescriber) en un solo string
            var errorMessages = string.Join(" ", result.Errors.Select(e => e.Description));

            // 2. Devolvemos un objeto JSON simple con el mensaje
            return BadRequest(new { message = errorMessages });
        }

        //Validamos el rol que llega. Si es inválido o vacío, asigna 'User'
        var roleToAdd = "User"; // Rol por defecto
        if (!string.IsNullOrEmpty(model.Role) && model.Role == "Admin")
        {
            roleToAdd = "Admin";
        }
        // Forzamos la exclusión mutua
        // Si vamos a agregar "Admin", nos aseguramos de quitar "User"
        if (roleToAdd == "Admin")
        {
            await _userManager.RemoveFromRoleAsync(user, "User");
        }
        // Si vamos a agregar "User", nos aseguramos de quitar "Admin"
        else
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        // Agregamos el rol deseado
        await _userManager.AddToRoleAsync(user, roleToAdd);
        return Ok("Usuario Registrado con Exito");

    }

    [HttpPost("register-customer")]
    [AllowAnonymous] // Público
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterModel model)
    {
        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = errorMessages });
        }

        // Forzamos el rol "User"
        var roleToAdd = "User";

        // Nos aseguramos de que no tenga el rol "Admin" (exclusión)
        await _userManager.RemoveFromRoleAsync(user, "Admin");
        await _userManager.AddToRoleAsync(user, roleToAdd);

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

