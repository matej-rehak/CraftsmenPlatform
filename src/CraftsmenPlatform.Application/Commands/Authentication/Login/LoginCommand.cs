using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthenticationResponse>>;