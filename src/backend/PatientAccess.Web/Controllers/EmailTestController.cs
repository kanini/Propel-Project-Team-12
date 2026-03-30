using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.Services;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Test controller for verifying email functionality (DEVELOPMENT ONLY).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Allow access without authentication for testing
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailTestController> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmailTestController(
        IEmailService emailService,
        ILogger<EmailTestController> logger,
        IWebHostEnvironment environment)
    {
        _emailService = emailService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Test endpoint to send a verification email (DEVELOPMENT ONLY).
    /// </summary>
    /// <param name="email">Email address to send test email to</param>
    /// <returns>Result of email send attempt</returns>
    [HttpPost("send-test-verification")]
    public async Task<IActionResult> SendTestVerificationEmail([FromQuery] string email)
    {
        // Only allow in development environment
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email parameter is required" });
        }

        try
        {
            _logger.LogInformation("Testing verification email send to {Email}", email);

            var testToken = Guid.NewGuid().ToString();
            var success = await _emailService.SendVerificationEmailAsync(
                email,
                "Test User",
                testToken);

            if (success)
            {
                return Ok(new
                {
                    message = "Test verification email sent successfully",
                    email,
                    token = testToken
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    message = "Failed to send test email. Check server logs for details."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test verification email to {Email}", email);
            return StatusCode(500, new
            {
                message = "Error sending test email",
                error = ex.Message,
                details = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Test endpoint to send an appointment confirmation email (DEVELOPMENT ONLY).
    /// </summary>
    /// <param name="email">Email address to send test email to</param>
    /// <returns>Result of email send attempt</returns>
    [HttpPost("send-test-appointment-confirmation")]
    public async Task<IActionResult> SendTestAppointmentConfirmation([FromQuery] string email)
    {
        // Only allow in development environment
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email parameter is required" });
        }

        try
        {
            _logger.LogInformation("Testing appointment confirmation email send to {Email}", email);

            // Create a small test PDF
            var testPdfBytes = System.Text.Encoding.UTF8.GetBytes(
                "%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj 2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj 3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R/Resources<<>>>>endobj\nxref\n0 4\n0000000000 65535 f\n0000000009 00000 n\n0000000056 00000 n\n0000000115 00000 n\ntrailer<</Size 4/Root 1 0 R>>\nstartxref\n206\n%%EOF");

            var success = await _emailService.SendAppointmentConfirmationAsync(
                email,
                "Test Patient",
                "Dr. Test Provider",
                DateTime.Now.AddDays(7),
                "TEST-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                testPdfBytes,
                "Test_Confirmation.pdf");

            if (success)
            {
                return Ok(new
                {
                    message = "Test appointment confirmation email sent successfully",
                    email
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    message = "Failed to send test email. Check server logs for details."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test appointment confirmation email to {Email}", email);
            return StatusCode(500, new
            {
                message = "Error sending test email",
                error = ex.Message,
                details = ex.InnerException?.Message
            });
        }
    }

    /// <summary>
    /// Gets SMTP configuration status (DEVELOPMENT ONLY).
    /// </summary>
    [HttpGet("smtp-status")]
    public IActionResult GetSmtpStatus()
    {
        // Only allow in development environment
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        return Ok(new
        {
            host = config["SmtpSettings:Host"],
            port = config["SmtpSettings:Port"],
            enableSsl = config["SmtpSettings:EnableSsl"],
            username = config["SmtpSettings:Username"],
            senderEmail = config["SmtpSettings:SenderEmail"],
            senderName = config["SmtpSettings:SenderName"],
            passwordConfigured = !string.IsNullOrWhiteSpace(config["SmtpSettings:Password"]) &&
                                 !config["SmtpSettings:Password"]!.Contains("SET_VIA_ENV")
        });
    }
}
