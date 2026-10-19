using Entities;
using RepositoryContracts;
using System.Text.Json;
namespace FileRepositories;

public class CommentFileRepository : ICommentRepository
{
    private readonly string filePath = "comments.json";

    public CommentFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        string commentsAsJson = await File.ReadAllTextAsync(filePath);

        List<Comment> comments =
            JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        int maxId = comments.Count > 0
            ? comments.Max(c => c.Id)
            : 0;

        comment.Id = maxId + 1;
        comments.Add(comment);

        commentsAsJson = JsonSerializer.Serialize(comments);

        await File.WriteAllTextAsync(filePath, commentsAsJson);

        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        string commentsAsJson = await File.ReadAllTextAsync(filePath);

        List<Comment> comments =
            JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        Comment? existingComment =
            comments.SingleOrDefault(c => c.Id == comment.Id);
        if (existingComment is null)
        {
            throw new EntityNotFoundException(
                $"Comment with ID '{comment.Id}' not found");
        }
        comments.Remove(existingComment);
        comments.Add(comment);
        commentsAsJson = JsonSerializer.Serialize(comments);
        await File.WriteAllTextAsync(filePath, commentsAsJson);
    }

    public async Task DeleteAsync(int id)
    {
        string commentsAsJson = await File.ReadAllTextAsync(filePath);

        List<Comment> comments =
            JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        Comment? commentToRemove =
            comments.SingleOrDefault(c => c.Id == id);

        if (commentToRemove is null)
        {
            throw new EntityNotFoundException(
                $"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);

        commentsAsJson = JsonSerializer.Serialize(comments);
        await File.WriteAllTextAsync(filePath, commentsAsJson);
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        string commentsAsJson = await File.ReadAllTextAsync(filePath);
        List<Comment> comments =
            JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");
        Comment? existingComment =
            comments.SingleOrDefault(c => c.Id == id);
        if (existingComment is null)
        {
            throw new EntityNotFoundException(
                $"Comment with ID '{id}' not found");
        }
        return existingComment;
    }

    public IQueryable<Comment> GetMany()
    {
        // Keep the synchronous interface while letting file errors reach the CLI directly.
        string commentsAsJson = File.ReadAllTextAsync(filePath).GetAwaiter().GetResult();

        List<Comment> comments =
            JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        return comments.AsQueryable();
    }
}
