
using System.Net;
using System.Text.Json;
using Dsw2025Tpi.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Api.Middleware;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // para pasar al siguiente middleware
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando la request: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // mapear excepciones personalizadas a códigos HTTP
        var statusCode = exception switch
        {
            BadRequestException => (int)HttpStatusCode.BadRequest,
            UnauthorizedException => (int)HttpStatusCode.Unauthorized,
            EntityNotFoundException => (int)HttpStatusCode.NotFound,
            DuplicatedEntityException => (int)HttpStatusCode.Conflict,
            ConflictException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };

        // manejar las excep que atrapa el mwr con logs 
        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Error inesperado en {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Excepcion controlada : {Message}", exception.Message);
        }

        object responsePayload;

        if (exception is BadRequestException badRequestEx && !string.IsNullOrEmpty(badRequestEx.ErrorCode))
        {
            responsePayload = new { message = exception.Message, code = badRequestEx.ErrorCode };
        }
        else
        {
            responsePayload = new { message = exception.Message, code = exception.GetType().Name };
        }

        var payload = JsonSerializer.Serialize(responsePayload);


        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(payload);
    }
}
