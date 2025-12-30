using CraftsmenPlatform.Domain.Events;
using CraftsmenPlatform.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CraftsmenPlatform.Application.Events;

/// <summary>
/// Handler pro UserRegisteredEvent - odesílá welcome email novým uživatelům
/// </summary>
public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        IEmailService emailService,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UserRegisteredEvent for user {UserId} ({Email})",
            @event.UserId,
            @event.Email);

        try
        {
            var fullName = $"{@event.FirstName} {@event.LastName}";
            
            var result = await _emailService.SendWelcomeEmailAsync(
                @event.Email,
                fullName,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to send welcome email to {Email}: {Error}",
                    @event.Email,
                    result.Error);
            }
            else
            {
                _logger.LogInformation(
                    "Welcome email sent successfully to {Email}",
                    @event.Email);
            }

            _logger.LogInformation("Sending welcome email to {Email}", @event.Email);
        }
        catch (Exception ex)
        {
            // Event handlery by neměly throwovat exceptions
            // Logujeme a pokračujeme
            _logger.LogError(ex,
                "Exception occurred while sending welcome email to {Email}",
                @event.Email);
        }
    }
}
