
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

namespace Dsw2025Tpi.Api;

public class Program
{

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);




        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TPI-DSW2025", Version = "v1" });

            
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

            // Para evitar conflictos con clases anidadas como record Response/Request
            c.CustomSchemaIds(type => type.FullName.Replace("+", "."));
        });


        builder.Services.AddHealthChecks();

        // registro antes porque utiliza cookies por defecto como esquema, y luego uso jwt
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {

            options.Password = new PasswordOptions
            {
                RequiredLength = 8
            };

        }).AddEntityFrameworkStores<AuthenticateContext>().AddDefaultTokenProviders();

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

            }); // esquema para servicio de autenticacion

       
        builder.Services.AddSingleton<JwtTokenService>();

        

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

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            context.LoadData(context);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}

