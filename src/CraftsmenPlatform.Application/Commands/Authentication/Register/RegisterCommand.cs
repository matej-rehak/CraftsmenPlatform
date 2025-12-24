using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role) : IRequest<Result<AuthenticationResponse>>;