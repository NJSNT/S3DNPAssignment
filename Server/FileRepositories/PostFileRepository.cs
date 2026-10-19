using Entities;
using RepositoryContracts;
using System.Text.Json;

namespace FileRepositories;

public class PostFileRepository : IPostRepository
{
    private readonly string filePath = "posts.json";

    public PostFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public async Task<Post> AddAsync(Post post)
    {
        string postsAsJson = await File.ReadAllTextAsync(filePath);

        List<Post> posts =
            JsonSerializer.Deserialize<List<Post>>(postsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        int maxId = posts.Count > 0
            ? posts.Max(p => p.Id)
            : 0;

        post.Id = maxId + 1;
        posts.Add(post);

        postsAsJson = JsonSerializer.Serialize(posts);
        await File.WriteAllTextAsync(filePath, postsAsJson);

        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        string postsAsJson = await File.ReadAllTextAsync(filePath);

        List<Post> posts =
            JsonSerializer.Deserialize<List<Post>>(postsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        Post? existingPost =
            posts.SingleOrDefault(p => p.Id == post.Id);

        if (existingPost is null)
        {
            throw new EntityNotFoundException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);

        postsAsJson = JsonSerializer.Serialize(posts);
        await File.WriteAllTextAsync(filePath, postsAsJson);
    }

    public async Task DeleteAsync(int id)
    {
        string postsAsJson = await File.ReadAllTextAsync(filePath);

        List<Post> posts =
            JsonSerializer.Deserialize<List<Post>>(postsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        Post? postToRemove =
            posts.SingleOrDefault(p => p.Id == id);

        if (postToRemove is null)
        {
            throw new EntityNotFoundException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);

        postsAsJson = JsonSerializer.Serialize(posts);
        await File.WriteAllTextAsync(filePath, postsAsJson);
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        string postsAsJson = await File.ReadAllTextAsync(filePath);

        List<Post> posts =
            JsonSerializer.Deserialize<List<Post>>(postsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        Post? post = posts.SingleOrDefault(p => p.Id == id);

        if (post is null)
        {
            throw new EntityNotFoundException(
                $"Post with ID '{id}' not found");
        }

        return post;
    }

    public IQueryable<Post> GetMany()
    {
        // Keep the synchronous interface while letting file errors reach the CLI directly.
        string postsAsJson = File.ReadAllTextAsync(filePath).GetAwaiter().GetResult();

        List<Post> posts =
            JsonSerializer.Deserialize<List<Post>>(postsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        return posts.AsQueryable();
    }
}
