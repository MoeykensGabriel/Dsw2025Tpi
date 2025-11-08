using Dsw2025Tpi.Api.Middleware;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TPI-DSW2025", Version = "v1" });

            // para ordenar endpoint por prioridad
            c.OrderActionsBy(apiDesc =>
            {
                var verbOrder = apiDesc.HttpMethod switch
                {
                    "GET" => 1,
                    "POST" => 2,
                    "PUT" => 3,
                    "PATCH" => 4,
                    "DELETE" => 5,
                    _ => 6
                };

                var isById = apiDesc.RelativePath.Contains("{id}") ? 2 : 1;

                return $"{verbOrder}_{isById}_{apiDesc.RelativePath}";
            });

            //  para seguridad JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer.\r\n\r\n" +
                              "Ejemplo: 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

            // para evitar conflictss con clases anidadas como record Response/Request
            c.CustomSchemaIds(type => type.FullName.Replace("+", "."));
        });

        builder.Services.AddHealthChecks();

        builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantiene los nombres tal cual
    });

        // registro antes porque utiliza cookies por defecto como esquema, y luego uso jwt
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {

            options.Password = new PasswordOptions
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
                RequireLowercase = true,
            };
            options.User.RequireUniqueEmail = true;

        })
            .AddEntityFrameworkStores<AuthenticateContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<SpanishIdentityErrorDescriber>();

        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("JWT Key");
        var key = Encoding.UTF8.GetBytes(keyText);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                // para la aplicacion de log de errores de autenticacion (no en la clase Middleware)
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Autenticación fallida: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Request no autorizado a {Path}", context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Request prohibido a {Path}", context.Request.Path);
                        return Task.CompletedTask;
                    }
                };

            }); // esquema para servicip de autenticacion

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantiene los nombres tal cual

                // Esto convierte los Enums (como OrderStatus) en strings (ej: "PENDING")
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
             });


        builder.Services.AddScoped<JwtTokenService>();

        builder.Services.AddDbContext<AuthenticateContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddScoped<Dsw2025TpiContext>();
        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Dsw2025Tpi.Data"));
        });
        builder.Services.AddScoped<IRepository, EfRepository>();
        builder.Services.AddScoped<ProductsManagementService>();
        builder.Services.AddScoped<OrdersManagementService>();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            Console.WriteLine($" Request: {context.Request.Method} {context.Request.Path}");
            Console.WriteLine($" Content-Type: {context.Request.ContentType}");
            await next();
            Console.WriteLine($" Response: {context.Response.StatusCode}");
        });


        app.UseMiddleware<ExceptionMiddleware>();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));
        }

        using (var scope = app.Services.CreateScope())
        {
            // 1. Obtenemos el ContentRootPath (la carpeta donde se ejecuta la API)
            var contentRoot = app.Environment.ContentRootPath;

            // 2. Combinamos la ruta base con la ruta relativa de tu archivo JSON
            //    Esto crea una ruta segura como: C:\...\Dsw2025Tpi.Api\DataSeed\customers.json
            var jsonFilePath = Path.Combine(contentRoot, "DataSeed", "customers.json");

            // 3. Pasamos la ruta dinámica al método LoadData
            var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            context.LoadData(context, jsonFilePath);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}
