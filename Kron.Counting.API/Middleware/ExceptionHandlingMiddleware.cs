using System.Net;
using System.Text.Json;
using FluentValidation;
using Kron.Counting.Shared.Exceptions;
using Kron.Counting.Shared.Responses;

namespace Kron.Counting.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var response = new ErrorResponse();
        var statusCode = HttpStatusCode.InternalServerError;
        var errorCode = "INTERNAL_ERROR";

        switch (exception)
        {
            case FluentValidation.ValidationException fluentValidationException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                response.Message = "Validation failed";
                response.Errors = fluentValidationException.Errors
                    .Select(x => x.ErrorMessage);
                break;

            case Kron.Counting.Shared.Exceptions.ValidationException sharedValidationException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                response.Message = "Validation failed";
                response.Errors = sharedValidationException.Errors;
                break;

            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                response.Message = exception.Message;
                break;

            case UnauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                response.Message = exception.Message;
                break;

            case ConflictException:
                statusCode = HttpStatusCode.Conflict;
                errorCode = "CONFLICT";
                response.Message = exception.Message;
                break;

            case AppException appException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "APP_ERROR";
                response.Message = appException.Message;
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                response.Message = exception.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                response.Message = exception.Message;
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "BAD_REQUEST";
                response.Message = exception.Message;
                break;

            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "BAD_REQUEST";
                response.Message = exception.Message;
                break;

            case FormatException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "BAD_REQUEST";
                response.Message = exception.Message;
                break;

            case HttpRequestException:
                statusCode = HttpStatusCode.BadGateway;
                errorCode = "BAD_GATEWAY";
                response.Message = exception.Message;
                break;

            default:
                response.Message = "An unexpected error occurred.";
                break;
        }

        response.Success = false;
        response.StatusCode = (int)statusCode;
        response.ErrorCode = errorCode;
        response.TraceId = context.TraceIdentifier;
        response.TimestampUtc = DateTime.UtcNow;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
