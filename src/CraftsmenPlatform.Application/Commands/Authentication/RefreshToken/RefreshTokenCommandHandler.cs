using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Domain.Common.Interfaces;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.RefreshToken;

public class RefreshTokenCommandHandler 
    : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRequestContext _requestContext;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IRequestContext requestContext)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _requestContext = requestContext;
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
            return Result<AuthenticationResponse>.Failure("Invalid refresh token");

        // 2. Find specific refresh token
        var refreshToken = user.RefreshTokens
            .FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (refreshToken is null)
            return Result<AuthenticationResponse>.Failure("Invalid refresh token");

        // 3. Validate refresh token
        if (!refreshToken.IsActive)
            return Result<AuthenticationResponse>.Failure(
                refreshToken.IsExpired
                    ? "Refresh token has expired"
                    : "Refresh token has been revoked");

        // 4. Validate user
        if (user.IsDeactivated)
            return Result<AuthenticationResponse>.Failure("Account is deactivated");

        if (user.IsLockedOut)
            return Result<AuthenticationResponse>.Failure(
                $"Account is locked until {user.LockedOutUntil:yyyy-MM-dd HH:mm:ss} UTC");

        // 5. Generate new tokens
        var ipAddress = _requestContext.GetIpAddress();

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // 6. Rotate refresh token (DOMAIN LOGIC)
        refreshToken.Revoke(ipAddress, newRefreshTokenValue);

        user.AddRefreshToken(
            newRefreshTokenValue,
            newRefreshTokenExpiresAt,
            ipAddress);

        // 7. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Response
        return Result<AuthenticationResponse>.Success(new AuthenticationResponse
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
            RefreshTokenExpiresAt = newRefreshTokenExpiresAt
        });
    }
}