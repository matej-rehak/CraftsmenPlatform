namespace CraftsmenPlatform.Application.Common.Settings;

public class MailjetSettings
{
    public const string SectionName = "Mailjet";

    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}