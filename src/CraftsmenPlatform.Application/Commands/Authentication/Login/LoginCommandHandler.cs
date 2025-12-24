using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.ValueObjects;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.Login;

public class LoginCommandHandler 
    : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var emailAddress = EmailAddress.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(
            emailAddress,
            cancellationToken);

        if (user is null)
            return Result.Failure<AuthenticationResponse>(
                "Invalid email or password");

        // 2. Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Record failed login attempt
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result.Failure<AuthenticationResponse>(
                "Invalid email or password");
        }

        // 3. Check if account is locked
        if (user.IsLockedOut)
            return Result.Failure<AuthenticationResponse>(
                $"Account is locked until {user.LockedOutUntil:yyyy-MM-dd HH:mm:ss} UTC");

        // 4. Check if account is deactivated
        if (user.IsDeactivated)
            return Result.Failure<AuthenticationResponse>(
                "Account is deactivated");

        // 5. Record successful login
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() 
            ?? "unknown";
        
        var loginResult = user.RecordSuccessfulLogin(ipAddress);
        if (loginResult.IsFailure)
            return Result.Failure<AuthenticationResponse>(loginResult.Error);

        // 6. Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            ipAddress);

        user.AddRefreshToken(refreshToken);

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
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = refreshToken.ExpiresAt
        });
    }
}