using CraftsmenPlatform.Application.Common.Interfaces;
using CraftsmenPlatform.Application.DTOs.Authentication;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Enums;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Domain.ValueObjects;
using CraftsmenPlatform.Domain.Common.Interfaces;
using MediatR;

namespace CraftsmenPlatform.Application.Commands.Authentication.Register;

public class RegisterCommandHandler 
    : IRequestHandler<RegisterCommand, Result<AuthenticationResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRequestContext _requestContext;

    public RegisterCommandHandler(
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
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var email = EmailAddress.Create(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
            return Result<AuthenticationResponse>.Failure(
                "User with this email already exists");

        // 2. Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 3. Parse role
        if (!Enum.TryParse<UserRole>(request.Role, out var role))
            return Result<AuthenticationResponse>.Failure("Invalid role");

        // 4. Create user
        var user = role switch
        {
            UserRole.Craftsman => User.CreateCraftsman(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName),

            UserRole.User => User.CreateUser(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName),
        };

        // 5. Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var ipAddress = _requestContext.GetIpAddress();

        // 🔥 JEDINÉ SPRÁVNÉ MÍSTO
        var refreshResult = user.AddRefreshToken(
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            ipAddress);

        if (refreshResult.IsFailure)
            return Result<AuthenticationResponse>.Failure(refreshResult.Error!);

        // 6. Save
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return
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
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }
}