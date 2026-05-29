using System.ComponentModel.DataAnnotations;

namespace simple_api.Application.Dtos;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record AuthResponse(string Token, string Email, DateTime ExpiresAt);
