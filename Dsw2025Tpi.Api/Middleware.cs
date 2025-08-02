using System.Net;
using System.Text.Json;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); //para pasar al siguiente middleware
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            //mapear excepciones personalizadas a códigos HTTP
            var statusCode = exception switch
            {
                BadRequestException => (int)HttpStatusCode.BadRequest,            
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,        
                EntityNotFoundException => (int)HttpStatusCode.NotFound,          
                DuplicatedEntityException => (int)HttpStatusCode.Conflict,        
                ConflictException => (int)HttpStatusCode.Conflict,               
                _ => (int)HttpStatusCode.InternalServerError                    
            };

            var response = new
            {
                message = exception.Message
            };

            var payload = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsync(payload);
        }
    }
}
