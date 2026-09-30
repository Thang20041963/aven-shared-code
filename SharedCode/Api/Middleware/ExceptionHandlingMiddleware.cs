using FluentValidation;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedCode.Contracts.Results;

namespace SharedBlocks.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, result) = exception switch
            {
                ValidationException validationEx => (
                    (int)HttpStatusCode.BadRequest,
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
                        ErrorType = ErrorType.Validation.ToString()
                    }
                ),

                KeyNotFoundException => (
                    (int)HttpStatusCode.NotFound,
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = exception.Message,
                        ErrorType = ErrorType.NotFound.ToString()
                    }
                ),

                UnauthorizedAccessException => (
                    (int)HttpStatusCode.Unauthorized,
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = exception.Message,
                        ErrorType = ErrorType.Unauthorized.ToString()
                    }
                ),

                InvalidOperationException => (
                    (int)HttpStatusCode.Conflict,
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = exception.Message,
                        ErrorType = ErrorType.Conflict.ToString()
                    }
                ),

                ArgumentException => (
                    (int)HttpStatusCode.BadRequest,
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = exception.Message,
                        ErrorType = ErrorType.Validation.ToString()
                    }
                ),

                TaskCanceledException or OperationCanceledException => (
                    499, // Client Closed Request
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = "The request was cancelled.",
                        ErrorType = ErrorType.Failure.ToString()
                    }
                ),

                TimeoutException => (
                    (int)HttpStatusCode.GatewayTimeout,
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = "The request timed out.",
                        ErrorType = ErrorType.Failure.ToString()
                    }
                ),

                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        IsSuccess = false,
                        Error = "An unexpected error occurred.",
                        ErrorType = ErrorType.Failure.ToString()
                    }
                )
            };

            // Enrich response with debug info
            result.Path = context.Request.Path;
            result.TraceId = context.TraceIdentifier;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            var json = JsonSerializer.Serialize(result, options);
            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Response format that matches the Result pattern used throughout the application.
    /// </summary>
    public class ErrorResponse
    {
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
        public string? ErrorType { get; set; }
        public object? Data { get; set; } = null;
        public string? Path { get; set; }
        public string? TraceId { get; set; }
    }
}
