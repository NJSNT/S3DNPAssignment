using System.Net.Http.Json;
using System.Security.Claims;
using ApiContracts;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Auth;

public class SimpleAuthProvider : AuthenticationStateProvider
{
    private readonly HttpClient httpClient;
    private ClaimsPrincipal currentUser = new(new ClaimsIdentity());

    public SimpleAuthProvider(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task LoginAsync(string userName, string password)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest
            {
                UserName = userName,
                Password = password
            });

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        UserDto user = await response.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("API returned no user.");

        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        ];

        currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "apiauth"));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public Task LogoutAsync()
    {
        currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(currentUser));
    }
}
