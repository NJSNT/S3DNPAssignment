using System.Net.Http.Json;
using ApiContracts;

namespace BlazorApp.HttpServices;

public class HttpCommentService : ICommentService
{
    private readonly HttpClient client;

    public HttpCommentService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<List<CommentDto>> GetCommentsForPostAsync(int postId)
    {
        return await client.GetFromJsonAsync<List<CommentDto>>($"Posts/{postId}/Comments")
            ?? throw new InvalidOperationException("API returned no comments.");
    }

    public async Task<CommentDto> AddCommentAsync(CreateCommentDto request)
    {
        using HttpResponseMessage response =
            await client.PostAsJsonAsync("Comments", request);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        return await response.Content.ReadFromJsonAsync<CommentDto>()
            ?? throw new InvalidOperationException("API returned no comment.");
    }
}
