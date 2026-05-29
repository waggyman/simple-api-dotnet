using System.ComponentModel.DataAnnotations;

namespace simple_api.Application.Dtos;

public sealed record TodoResponse(
    int Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public sealed record CreateTodoRequest(
    [Required, MinLength(1), MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description);

public sealed record UpdateTodoRequest(
    [MinLength(1), MaxLength(200)] string? Title,
    [MaxLength(2000)] string? Description,
    bool? IsCompleted);
