using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Domain.Services;

public interface IEmailService
{
    Task<Result> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);

    Task<Result> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default);

    Task<Result> SendEmailVerificationAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default);

    Task<Result> SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default);
}