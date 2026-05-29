using simple_api.Application.Common;
using simple_api.Application.Dtos;

namespace simple_api.Application.Contracts;

public interface ITodoService
{
    Task<IReadOnlyList<TodoResponse>> ListAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<TodoResponse>> GetByIdAsync(int userId, int todoId, CancellationToken cancellationToken = default);
    Task<Result<TodoResponse>> CreateAsync(int userId, CreateTodoRequest request, CancellationToken cancellationToken = default);
    Task<Result<TodoResponse>> UpdateAsync(int userId, int todoId, UpdateTodoRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int userId, int todoId, CancellationToken cancellationToken = default);
}
