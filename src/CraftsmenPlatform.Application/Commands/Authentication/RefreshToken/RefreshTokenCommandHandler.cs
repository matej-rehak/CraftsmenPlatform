using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftsmenPlatform.Application.Commands.Authentication.RefreshToken;

public class RefreshTokenCommandHandler 
    : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find user by refresh token
        var user = await _userRepository.GetByRefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (user is null)
            return Result.Failure<AuthenticationResponse>("Invalid refresh token");

        // 2. Find the specific refresh token
        var refreshToken = user.RefreshTokens
            .FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (refreshToken is null)
            return Result.Failure<AuthenticationResponse>("Invalid refresh token");

        // 3. Validate refresh token
        if (!refreshToken.IsActive)
            return Result.Failure<AuthenticationResponse>(
                refreshToken.IsExpired 
                    ? "Refresh token has expired" 
                    : "Refresh token has been revoked");

        // 4. Check if user account is valid
        if (user.IsDeactivated)
            return Result.Failure<AuthenticationResponse>("Account is deactivated");

        if (user.IsLockedOut)
            return Result.Failure<AuthenticationResponse>(
                $"Account is locked until {user.LockedOutUntil:yyyy-MM-dd HH:mm:ss} UTC");

        // 5. Generate new tokens
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() 
            ?? "unknown";

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            ipAddress);

        // 6. Revoke old refresh token and add new one
        refreshToken.Revoke(ipAddress, newRefreshTokenValue);
        user.AddRefreshToken(newRefreshToken);

        // 7. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Return response
        return Result.Success(new AuthenticationResponse
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = newRefreshToken.ExpiresAt
        });
    }
}