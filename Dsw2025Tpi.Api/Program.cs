
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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
                c.CustomSchemaIds(type => type.FullName.Replace("+", "."));
            });
            builder.Services.AddHealthChecks();
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

            app.UseAuthorization();

            app.MapControllers();

            app.MapHealthChecks("/healthcheck");

            app.Run();
        }
}

