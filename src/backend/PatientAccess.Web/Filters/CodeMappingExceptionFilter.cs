using Azure;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PatientAccess.Web.Filters;

/// <summary>
/// Exception filter for handling Azure OpenAI and validation failures in code mapping operations.
/// US_051 Task 2 - Code Mapping API.
/// Provides user-friendly error responses for AI service failures and schema validation errors.
/// </summary>
public class CodeMappingExceptionFilter : IExceptionFilter
{
    private readonly ILogger<CodeMappingExceptionFilter> _logger;

    public CodeMappingExceptionFilter(ILogger<CodeMappingExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        // Handle Azure OpenAI service failures (rate limits, timeouts, service unavailable)
        if (context.Exception is RequestFailedException azureException)
        {
            _logger.LogError(azureException,
                "Azure OpenAI request failed with status {Status}: {Message}",
                azureException.Status, azureException.Message);

            var statusCode = azureException.Status switch
            {
                429 => StatusCodes.Status429TooManyRequests, // Rate limit exceeded
                503 => StatusCodes.Status503ServiceUnavailable, // Service unavailable
                _ => StatusCodes.Status503ServiceUnavailable // Default to service unavailable
            };

            context.Result = new ObjectResult(new
            {
                Error = "AI service temporarily unavailable. Please try again later.",
                Details = azureException.Message,
                RetryAfter = statusCode == 429 ? "60 seconds" : null
            })
            {
                StatusCode = statusCode
            };
            context.ExceptionHandled = true;
        }
        // Handle FluentValidation schema validation failures (AIR-Q03: output schema validity)
        else if (context.Exception is ValidationException validationException)
        {
            _logger.LogWarning(validationException,
                "Code mapping response validation failed: {Errors}",
                string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage)));

            context.Result = new ObjectResult(new
            {
                Error = "AI response validation failed. Code mapping quality below threshold.",
                ValidationErrors = validationException.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage,
                    AttemptedValue = e.AttemptedValue
                })
            })
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
            context.ExceptionHandled = true;
        }
        // Handle invalid operation exceptions (e.g., JSON parsing failures from LLM)
        else if (context.Exception is InvalidOperationException invalidOpException)
        {
            _logger.LogError(invalidOpException,
                "Invalid operation in code mapping: {Message}",
                invalidOpException.Message);

            context.Result = new ObjectResult(new
            {
                Error = "Code mapping operation failed. Invalid AI response format.",
                Details = invalidOpException.Message
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
        }
        // Handle timeout exceptions (Polly retry exhausted)
        else if (context.Exception is TimeoutException timeoutException)
        {
            _logger.LogError(timeoutException,
                "Code mapping request timed out: {Message}",
                timeoutException.Message);

            context.Result = new ObjectResult(new
            {
                Error = "Code mapping request timed out. AI service took too long to respond.",
                Details = "Please try again with shorter clinical text or contact support if issue persists."
            })
            {
                StatusCode = StatusCodes.Status504GatewayTimeout
            };
            context.ExceptionHandled = true;
        }
    }
}
