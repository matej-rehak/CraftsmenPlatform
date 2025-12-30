using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Services;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CraftsmenPlatform.Application.Common.Settings;

namespace CraftsmenPlatform.Infrastructure.Services;

public class MailjetEmailService : IEmailService
{
    private readonly MailjetClient _client;
    private readonly MailjetSettings _settings;
    private readonly ILogger _logger;

    public MailjetEmailService(
        IOptions<MailjetSettings> settings,
        ILogger<MailjetEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new MailjetClient(_settings.ApiKey, _settings.ApiSecret);
    }

    public async Task<Result> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(_settings.SenderEmail, _settings.SenderName))
                .WithTo(new SendContact(toEmail, toName))
                .WithSubject(subject)
                .WithHtmlPart(htmlBody)
                .WithTextPart(textBody ?? string.Empty)
                .Build();

            var response = await _client.SendTransactionalEmailAsync(email);

            if (response.Messages == null || response.Messages.Length == 0)
            {
                _logger.LogError("Mailjet returned no messages in response");
                return Result.Failure("Failed to send email: No response from email service");
            }

            var message = response.Messages[0];
            
            if (message.Status == "success")
            {
                _logger.LogInformation(
                    "Email sent successfully to {Email}. MessageID: {MessageId}",
                    toEmail,
                    message.To?[0].MessageID);
                
                return Result.Success();
            }

            _logger.LogError(
                "Failed to send email to {Email}. Status: {Status}",
                toEmail,
                message.Status);
            
            return Result.Failure($"Failed to send email: {message.Status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            return Result.Failure($"Email service error: {ex.Message}");
        }
    }

    public async Task<Result> SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var subject = "Vítejte na CraftsmenPlatform!";
        var htmlBody = $@"
            
            
                Vítejte, {userName}!
                Děkujeme za registraci na CraftsmenPlatform.
                Nyní můžete začít využívat všechny funkce naší platformy:
                
                    Vytvářet projekty a získávat nabídky od řemeslníků
                    Prohlížet profily ověřených řemeslníků
                    Komunikovat přes bezpečný chat
                    Hodnotit dokončené práce
                
                S pozdravem,Tým CraftsmenPlatform
            
            ";

        var textBody = $@"
Vítejte, {userName}!

Děkujeme za registraci na CraftsmenPlatform.

Nyní můžete začít využívat všechny funkce naší platformy.

S pozdravem,
Tým CraftsmenPlatform";

        return await SendEmailAsync(toEmail, userName, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task<Result> SendEmailVerificationAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Ověření emailové adresy";
        var htmlBody = $@"
            
            
                Ahoj, {userName}!
                Prosím, ověřte svou emailovou adresu kliknutím na následující odkaz:
                Ověřit Email
                Pokud jste se neregistrovali na CraftsmenPlatform, ignorujte tento email.
                Odkaz je platný 24 hodin.
                S pozdravem,Tým CraftsmenPlatform
            
            ";

        var textBody = $@"
Ahoj, {userName}!

Prosím, ověřte svou emailovou adresu kliknutím na následující odkaz:
{verificationLink}

Pokud jste se neregistrovali na CraftsmenPlatform, ignorujte tento email.
Odkaz je platný 24 hodin.

S pozdravem,
Tým CraftsmenPlatform";

        return await SendEmailAsync(toEmail, userName, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task<Result> SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Obnovení hesla";
        var htmlBody = $@"
            
            
                Ahoj, {userName}!
                Obdrželi jsme požadavek na obnovení hesla k vašemu účtu.
                Pro obnovení hesla klikněte na následující odkaz:
                Obnovit Heslo
                Pokud jste o obnovu hesla nežádali, ignorujte tento email.
                Odkaz je platný 1 hodinu.
                S pozdravem,Tým CraftsmenPlatform
            
            ";

        var textBody = $@"
Ahoj, {userName}!

Obdrželi jsme požadavek na obnovení hesla k vašemu účtu.

Pro obnovení hesla použijte následující odkaz:
{resetLink}

Pokud jste o obnovu hesla nežádali, ignorujte tento email.
Odkaz je platný 1 hodinu.

S pozdravem,
Tým CraftsmenPlatform";

        return await SendEmailAsync(toEmail, userName, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task<Result> SendOfferNotificationAsync(
        string toEmail,
        string customerName,
        string craftsmanName,
        string projectTitle,
        decimal offerAmount,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Nová nabídka na projekt: {projectTitle}";
        var htmlBody = $@"
            
            
                Ahoj, {customerName}!
                Obdrželi jste novou nabídku na váš projekt {projectTitle}.
                
                    Řemeslník: {craftsmanName}
                    Nabídková cena: {offerAmount:N0} Kč
                
                Přihlaste se do svého účtu pro zobrazení detailů nabídky.
                S pozdravem,Tým CraftsmenPlatform
            
            ";

        var textBody = $@"
Ahoj, {customerName}!

Obdrželi jste novou nabídku na váš projekt {projectTitle}.

Řemeslník: {craftsmanName}
Nabídková cena: {offerAmount:N0} Kč

Přihlaste se do svého účtu pro zobrazení detailů nabídky.

S pozdravem,
Tým CraftsmenPlatform";

        return await SendEmailAsync(toEmail, customerName, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task<Result> SendOfferAcceptedNotificationAsync(
        string toEmail,
        string craftsmanName,
        string projectTitle,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Vaše nabídka byla přijata: {projectTitle}";
        var htmlBody = $@"
            
            
                Gratulujeme, {craftsmanName}!
                Vaše nabídka na projekt {projectTitle} byla přijata.
                Zákazník si vybral vaši nabídku. Můžete nyní zahájit komunikaci a realizaci projektu.
                Přihlaste se do svého účtu pro další detaily.
                S pozdravem,Tým CraftsmenPlatform
            
            ";

        var textBody = $@"
Gratulujeme, {craftsmanName}!

Vaše nabídka na projekt {projectTitle} byla přijata.

Zákazník si vybral vaši nabídku. Můžete nyní zahájit komunikaci a realizaci projektu.

Přihlaste se do svého účtu pro další detaily.

S pozdravem,
Tým CraftsmenPlatform";

        return await SendEmailAsync(toEmail, craftsmanName, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task<Result> SendProjectCompletedNotificationAsync(
        string toEmail,
        string customerName,
        string projectTitle,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Projekt dokončen: {projectTitle}";
        var htmlBody = $@"
            
            
                Ahoj, {customerName}!
                Projekt {projectTitle} byl označen jako dokončený.
                Pokud jste s prací spokojeni, zanechte prosím hodnocení řemeslníka. Vaše zpětná vazba pomůže ostatním zákazníkům.
                Přihlaste se do svého účtu pro hodnocení projektu.
                S pozdravem,Tým CraftsmenPlatform
            
            ";

        var textBody = $@"
Ahoj, {customerName}!

Projekt {projectTitle} byl označen jako dokončený.

Pokud jste s prací spokojeni, zanechte prosím hodnocení řemeslníka.
Vaše zpětná vazba pomůže ostatním zákazníkům.

Přihlaste se do svého účtu pro hodnocení projektu.

S pozdravem,
Tým CraftsmenPlatform";

        return await SendEmailAsync(toEmail, customerName, subject, htmlBody, textBody, cancellationToken);
    }
}