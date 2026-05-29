using Microsoft.EntityFrameworkCore;
using simple_api.Application.Common;
using simple_api.Application.Contracts;
using simple_api.Application.Dtos;
using simple_api.Domain.Entities;
using simple_api.Infrastructure.Persistence;

namespace simple_api.Application.Services;

public class TodoService(AppDbContext dbContext) : ITodoService
{
    public async Task<IReadOnlyList<TodoResponse>> ListAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var todos = await dbContext.Todos
            .AsNoTracking()
            .Where(todo => todo.UserId == userId)
            .OrderByDescending(todo => todo.CreatedAt)
            .ToListAsync(cancellationToken);

        return todos.Select(MapToResponse).ToList();
    }

    public async Task<Result<TodoResponse>> GetByIdAsync(
        int userId,
        int todoId,
        CancellationToken cancellationToken = default)
    {
        var todo = await FindOwnedTodoAsync(userId, todoId, cancellationToken);

        return todo is null
            ? Result<TodoResponse>.Fail("Todo not found.")
            : Result<TodoResponse>.Ok(MapToResponse(todo));
    }

    public async Task<Result<TodoResponse>> CreateAsync(
        int userId,
        CreateTodoRequest request,
        CancellationToken cancellationToken = default)
    {
        var todo = new Todo
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<TodoResponse>.Ok(MapToResponse(todo));
    }

    public async Task<Result<TodoResponse>> UpdateAsync(
        int userId,
        int todoId,
        UpdateTodoRequest request,
        CancellationToken cancellationToken = default)
    {
        var todo = await FindOwnedTodoAsync(userId, todoId, cancellationToken, tracked: true);

        if (todo is null)
        {
            return Result<TodoResponse>.Fail("Todo not found.");
        }

        if (request.Title is not null)
        {
            todo.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            todo.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
        }

        if (request.IsCompleted is { } isCompleted)
        {
            todo.IsCompleted = isCompleted;
            todo.CompletedAt = isCompleted ? DateTime.UtcNow : null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<TodoResponse>.Ok(MapToResponse(todo));
    }

    public async Task<Result<bool>> DeleteAsync(
        int userId,
        int todoId,
        CancellationToken cancellationToken = default)
    {
        var todo = await FindOwnedTodoAsync(userId, todoId, cancellationToken, tracked: true);

        if (todo is null)
        {
            return Result<bool>.Fail("Todo not found.");
        }

        dbContext.Todos.Remove(todo);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }

    private async Task<Todo?> FindOwnedTodoAsync(
        int userId,
        int todoId,
        CancellationToken cancellationToken,
        bool tracked = false)
    {
        var query = tracked
            ? dbContext.Todos.AsQueryable()
            : dbContext.Todos.AsNoTracking();

        return await query
            .SingleOrDefaultAsync(todo => todo.Id == todoId && todo.UserId == userId, cancellationToken);
    }

    private static TodoResponse MapToResponse(Todo todo) =>
        new(todo.Id, todo.Title, todo.Description, todo.IsCompleted, todo.CreatedAt, todo.CompletedAt);
}
