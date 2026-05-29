using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using simple_api.Application.Dtos;

namespace simple_api.Tests;

public class TodoEndpointTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task TodoCrud_RequiresAuthAndScopesByUser()
    {
        using var client = factory.CreateClient();
        var token = await RegisterAndGetTokenAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync(
            "/api/todos",
            new CreateTodoRequest("Buy groceries", "Milk and eggs"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Buy groceries", created.Title);

        var listResponse = await client.GetAsync("/api/todos");
        listResponse.EnsureSuccessStatusCode();

        var todos = await listResponse.Content.ReadFromJsonAsync<List<TodoResponse>>(JsonOptions);
        Assert.NotNull(todos);
        Assert.Single(todos);

        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/todos/{created.Id}",
            new UpdateTodoRequest(null, null, true));

        patchResponse.EnsureSuccessStatusCode();

        var updated = await patchResponse.Content.ReadFromJsonAsync<TodoResponse>(JsonOptions);
        Assert.True(updated?.IsCompleted);

        var deleteResponse = await client.DeleteAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await client.GetAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task ListTodos_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/todos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<string> RegisterAndGetTokenAsync(HttpClient client)
    {
        var email = $"todo-{Guid.NewGuid():N}@example.com";
        const string password = "password123";

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password));

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return body?.Token ?? throw new InvalidOperationException("Token was not returned.");
    }
}
