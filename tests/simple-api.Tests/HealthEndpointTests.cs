namespace simple_api.Tests;

public class HealthEndpointTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task GetHealth_ReturnsHealthyWithoutAuth()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("Healthy", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sqlite", body, StringComparison.OrdinalIgnoreCase);
    }
}
