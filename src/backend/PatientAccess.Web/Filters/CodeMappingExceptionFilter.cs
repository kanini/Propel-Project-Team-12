using Azure;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PatientAccess.Web.Filters;

/// <summary>
/// Exception filter for medical code mapping endpoints.
/// Handles Azure OpenAI failures and schema validation errors with proper HTTP responses.
/// </summary>
public class CodeMappingExceptionFilter : IExceptionFilter
{
    private readonly ILogger<CodeMappingExceptionFilter> _logger;

    public CodeMappingExceptionFilter(ILogger<CodeMappingExceptionFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void OnException(ExceptionContext context)
    {
        // Handle Azure OpenAI API failures (circuit breaker open, rate limit, service unavailable)
        if (context.Exception is RequestFailedException azureException)
        {
            _logger.LogError(azureException, "Azure OpenAI request failed: {Message}", azureException.Message);
            
            context.Result = new ObjectResult(new
            {
                error = "AI service temporarily unavailable. Please try again later.",
                details = azureException.Message,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            
            context.ExceptionHandled = true;
        }
        // Handle FluentValidation schema validation failures (AIR-Q03)
        else if (context.Exception is ValidationException validationException)
        {
            _logger.LogWarning(validationException, "Code mapping response validation failed: {Errors}",
                string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage)));
            
            context.Result = new ObjectResult(new
            {
                error = "AI response validation failed. Code mapping quality below threshold.",
                validationErrors = validationException.Errors.Select(e => e.ErrorMessage).ToList(),
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
            
            context.ExceptionHandled = true;
        }
        // Handle file not found exceptions (missing prompt templates)
        else if (context.Exception is FileNotFoundException fileException)
        {
            _logger.LogError(fileException, "Required configuration file not found: {Message}", fileException.Message);
            
            context.Result = new ObjectResult(new
            {
                error = "Code mapping service configuration error. Please contact support.",
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            
            context.ExceptionHandled = true;
        }
    }
}
