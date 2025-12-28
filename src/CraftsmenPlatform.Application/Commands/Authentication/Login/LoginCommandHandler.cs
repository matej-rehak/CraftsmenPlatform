using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;
using CraftsmenPlatform.Domain.Common.Interfaces;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.Login;

public class LoginCommandHandler 
    : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRequestContext _requestContext;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRequestContext requestContext)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _requestContext = requestContext;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1.1 Validate email
            var emailResult = EmailAddress.Create(request.Email);
            if (emailResult.IsFailure)
                return Result<AuthenticationResponse>.Failure("Invalid email format");

            var emailAddress = emailResult.Value;

            // 2. Find user by email
            var user = await _userRepository.GetByEmailAsync(
                emailAddress,
                cancellationToken);

            if (user is null)
                return Result<AuthenticationResponse>.Failure(
                    "Invalid email or password");

            var ipAddress = _requestContext.GetIpAddress();

            // 3. Verify password
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                var recordFailedLoginResult = user.RecordFailedLogin(ipAddress);
                if (recordFailedLoginResult.IsFailure)
                    return Result<AuthenticationResponse>.Failure(
                        recordFailedLoginResult.Error);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<AuthenticationResponse>.Failure(
                    "Invalid email or password");
            }

            // 4. Record successful login (aktualizuje LastLoginAt na User entitě)
            var loginResult = user.RecordSuccessfulLogin(ipAddress);
            if (loginResult.IsFailure)
                return Result<AuthenticationResponse>.Failure(
                    loginResult.Error ?? "Login failed");

            // 5. Generate tokens
            string accessToken;
            string refreshTokenValue;
            DateTime refreshTokenExpiresAt;
            try
            {
                accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
                refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
                refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            }
            catch (Exception ex)
            {
                return Result<AuthenticationResponse>.Failure($"Failed to generate tokens: {ex.Message}");
            }

            // 6. Vytvoř RefreshToken entitu přes DDD factory
            var refreshTokenResult = CraftsmenPlatform.Domain.Entities.RefreshToken.Create(
                user.Id,
                refreshTokenValue,
                refreshTokenExpiresAt,
                ipAddress
            );

            if (refreshTokenResult.IsFailure)
                return Result<AuthenticationResponse>.Failure(
                    refreshTokenResult.Error ?? "Failed to create refresh token");

            var refreshToken = refreshTokenResult.Value;

            // 7. Přidej refresh token přímo do contextu (bez User tracking)
            await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);

            // 8. Ulož LastLoginAt bez concurrency check
            await _userRepository.UpdateLastLoginAsync(user.Id, cancellationToken);

            // 9. Return response
            return Result<AuthenticationResponse>.Success(new AuthenticationResponse
            {
                UserId = user.Id,
                Email = user.Email.Value,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified,
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            });
        }
        catch (Exception ex)
        {
            return Result<AuthenticationResponse>.Failure($"{ex.Message}");
        }
    }
}