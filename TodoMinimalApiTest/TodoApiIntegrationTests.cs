using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TodoMinimalApi.ViewModels;

namespace TodoMinimalApiTest;

public class TodoApiIntegrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateTodoItem(bool getToken = false)
    {
        if (!getToken)
        {
            var token = await GetTokenForUser1();
            Assert.NotNull(token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await _httpClient.PostAsync("/todos",
            new StringContent(JsonSerializer.Serialize(new TodoInput { Title = "Test", IsCompleted = false }),
                Encoding.UTF8, "application/json"));
        var responseStatusCode = response.StatusCode;
        if (!getToken)
        {
            Assert.Equal(HttpStatusCode.Created, responseStatusCode);
            Assert.NotNull(response.Headers.Location);
        }
        else
        {
            Assert.Equal(HttpStatusCode.Unauthorized, responseStatusCode);
        }
    }
    
    [Fact(Skip = "Running this test will exhaust the anonymous request limit - which fails the other tests")]
    public async Task GetHealthWithoutToken()
    {
        var response = await _httpClient.GetAsync("/health");
        var responseStatusCode = response.StatusCode;
        Assert.Equal(HttpStatusCode.OK, responseStatusCode);

        for (int i = 0; i < 29; i++)
        {
            response = await _httpClient.GetAsync("/health");
            responseStatusCode = response.StatusCode;
            Assert.Equal(HttpStatusCode.OK, responseStatusCode);
        }

        response = await _httpClient.GetAsync("/health");
        responseStatusCode = response.StatusCode;
        Assert.Equal(HttpStatusCode.TooManyRequests, responseStatusCode);
    }

    private static async Task<string> GetTokenForUser1()
    {
        var token = Environment.GetEnvironmentVariable("USER1_TOKEN") ?? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVzZXIxIiwic3ViIjoidXNlcjEiLCJqdGkiOiI4YzY5Y2NhNyIsIlVzZXJuYW1lIjoidXNlcjEiLCJFbWFpbCI6InVzZXIxQGV4YW1wbGUuY29tIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6Mzk5NDkiLCJodHRwczovL2xvY2FsaG9zdDo0NDM1OCIsImh0dHA6Ly9sb2NhbGhvc3Q6NTAyOCIsImh0dHBzOi8vbG9jYWxob3N0OjcwNzUiXSwibmJmIjoxNzAwNzk4NDEyLCJleHAiOjE3MDg3NDcyMTIsImlhdCI6MTcwMDc5ODQxMiwiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.ds6GX38NU1a4ywFS2vpfFK1TZJgaRfM1TE9r8uYc2qM";
        return await Task.Run(() => token);
    }
}