using System.Net.Http.Json;
using ApiContracts;

namespace BlazorApp.HttpServices;

public class HttpPostService : IPostService
{
    private readonly HttpClient client;

    public HttpPostService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<List<PostDto>> GetPostsAsync()
    {
        return await client.GetFromJsonAsync<List<PostDto>>("Posts")
            ?? throw new InvalidOperationException("API returned no posts.");
    }

    public async Task<PostDto> GetPostAsync(int id)
    {
        return await client.GetFromJsonAsync<PostDto>($"Posts/{id}")
            ?? throw new InvalidOperationException("API returned no post.");
    }

    public async Task<PostDto> AddPostAsync(CreatePostDto request)
    {
        using HttpResponseMessage response =
            await client.PostAsJsonAsync("Posts", request);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        return await response.Content.ReadFromJsonAsync<PostDto>()
            ?? throw new InvalidOperationException("API returned no post.");
    }
}
