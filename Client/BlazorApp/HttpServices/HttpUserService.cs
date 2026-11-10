using System.Net.Http.Json;
using ApiContracts;

namespace BlazorApp.HttpServices;

public class HttpUserService : IUserService
{
    private readonly HttpClient client;

    public HttpUserService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<UserDto> AddUserAsync(CreateUserDto request)
    {
        using HttpResponseMessage response =
            await client.PostAsJsonAsync("Users", request);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        return await response.Content.ReadFromJsonAsync<UserDto>()
               ?? throw new InvalidOperationException("API returned no user.");
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        return await client.GetFromJsonAsync<UserDto>($"Users/{id}")
            ?? throw new InvalidOperationException("API returned no user.");
    }
}
