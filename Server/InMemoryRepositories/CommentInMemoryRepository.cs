using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository : ICommentRepository
{
    private readonly List<Comment> comments = new List<Comment>
    {
        new Comment
        {
            Id = 1,
            Body = "Hello everyone!",
            UserId = 2,
            PostId = 1
        },
        new Comment
        {
            Id = 2,
            Body = "Happy to be here.",
            UserId = 3,
            PostId = 1
        },
        new Comment
        {
            Id = 3,
            Body = "Remember to await the repository methods.",
            UserId = 1,
            PostId = 2
        }
    };

    public Task<Comment> AddAsync(Comment comment)
    {
        if (comments.Any())
        {
            comment.Id = comments.Max(c => c.Id) + 1;
        }
        else
        {
            comment.Id = 1;
        }

        comments.Add(comment);
        return Task.FromResult(comment);
    }

    public Task UpdateAsync(Comment comment)
    {
        Comment? existingComment =
            comments.SingleOrDefault(c => c.Id == comment.Id);

        if (existingComment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{comment.Id}' not found");
        }

        comments.Remove(existingComment);
        comments.Add(comment);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Comment? commentToRemove =
            comments.SingleOrDefault(c => c.Id == id);

        if (commentToRemove is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        comments.Remove(commentToRemove);

        return Task.CompletedTask;
    }

    public Task<Comment> GetSingleAsync(int id)
    {
        Comment? comment =
            comments.SingleOrDefault(c => c.Id == id);

        if (comment is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID '{id}' not found");
        }

        return Task.FromResult(comment);
    }

    public IQueryable<Comment> GetMany()
    {
        return comments.AsQueryable();
    }
}
