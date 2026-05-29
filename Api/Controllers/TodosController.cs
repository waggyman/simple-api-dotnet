using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using simple_api.Api.Extensions;
using simple_api.Application.Contracts;
using simple_api.Application.Dtos;

namespace simple_api.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/todos")]
public class TodosController(ITodoService todoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TodoResponse>>> List(CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var todos = await todoService.ListAsync(userId, cancellationToken);
        return Ok(todos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoResponse>> Get(int id, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await todoService.GetByIdAsync(userId, id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<TodoResponse>> Create(
        [FromBody] CreateTodoRequest request,
        CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await todoService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<TodoResponse>> Update(
        int id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await todoService.UpdateAsync(userId, id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await todoService.DeleteAsync(userId, id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return NoContent();
    }
}
