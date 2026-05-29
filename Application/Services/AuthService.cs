using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using simple_api.Application.Common;
using simple_api.Application.Contracts;
using simple_api.Application.Dtos;
using simple_api.Domain.Entities;
using simple_api.Infrastructure.Persistence;
using simple_api.Infrastructure.Security;

namespace simple_api.Application.Services;

public class AuthService(
    AppDbContext dbContext,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await dbContext.Users
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            return Result<AuthResponse>.Fail("Email is already registered.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
            CreatedAt = DateTime.UtcNow,
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result<AuthResponse>.Fail("Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Result<AuthResponse>.Fail("Invalid email or password.");
        }

        return Result<AuthResponse>.Ok(BuildAuthResponse(user));
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes);
        var token = tokenService.CreateToken(user);
        return new AuthResponse(token, user.Email, expiresAt);
    }
}
