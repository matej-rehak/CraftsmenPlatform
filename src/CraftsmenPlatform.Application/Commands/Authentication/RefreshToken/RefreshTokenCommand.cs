using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) 
    : IRequest<Result<AuthenticationResponse>>;