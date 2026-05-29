using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using simple_api.Application.Dtos;

namespace simple_api.Tests;

public class AuthEndpointTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task RegisterAndLogin_ReturnsJwtToken()
    {
        using var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "password123";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(registerBody?.Token);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(loginBody?.Token);
    }

    [Fact]
    public async Task RegisterDuplicateEmail_ReturnsConflict()
    {
        using var client = factory.CreateClient();
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        const string password = "password123";

        var first = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
}
