using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Domain.ValueObjects;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.Register;

public class RegisterCommandHandler 
    : IRequestHandler<RegisterCommand, Result<AuthenticationResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegisterCommandHandler(
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
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var emailAddress = EmailAddress.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(
            emailAddress,
            cancellationToken);

        if (existingUser is not null)
            return Result.Failure<AuthenticationResponse>(
                "User with this email already exists");

        // 2. Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 3. Parse role
        if (!Enum.TryParse<UserRole>(request.Role, out var userRole))
            return Result.Failure<AuthenticationResponse>("Invalid role");

        // 4. Create user
        var user = userRole switch
        {
            UserRole.Craftsman => User.CreateCraftsman(
                emailAddress,
                passwordHash,
                request.FirstName,
                request.LastName),
            UserRole.Customer => User.CreateCustomer(
                emailAddress,
                passwordHash,
                request.FirstName,
                request.LastName),
            _ => User.CreateUser(
                emailAddress,
                passwordHash,
                request.FirstName,
                request.LastName)
        };

        // 5. Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() 
            ?? "unknown";
        
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            ipAddress);

        user.AddRefreshToken(refreshToken);

        // 6. Save to database
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return response
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