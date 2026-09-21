using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository : IPostRepository
{
    private readonly List<Post> posts = new List<Post>
    {
        new Post
        {
            Id = 1,
            Title = "Welcome",
            Body = "Welcome to our forum!",
            UserId = 1
        },
        new Post
        {
            Id = 2,
            Title = "Learning C#",
            Body = "I am learning async and await.",
            UserId = 2
        },
        new Post
        {
            Id = 3,
            Title = "DNP Assignment",
            Body = "Let us build a CLI together.",
            UserId = 3
        }
    };

    public Task<Post> AddAsync(Post post)
    {
        if (posts.Any())
        {
            post.Id = posts.Max(p => p.Id) + 1;
        }
        else
        {
            post.Id = 1;
        }

        posts.Add(post);
        return Task.FromResult(post);
    }

    public Task UpdateAsync(Post post)
    {
        Post? existingPost =
            posts.SingleOrDefault(p => p.Id == post.Id);

        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{post.Id}' not found");
        }

        posts.Remove(existingPost);
        posts.Add(post);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Post? postToRemove =
            posts.SingleOrDefault(p => p.Id == id);

        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        posts.Remove(postToRemove);

        return Task.CompletedTask;
    }

    public Task<Post> GetSingleAsync(int id)
    {
        Post? post = posts.SingleOrDefault(p => p.Id == id);

        if (post is null)
        {
            throw new InvalidOperationException(
                $"Post with ID '{id}' not found");
        }

        return Task.FromResult(post);
    }

    public IQueryable<Post> GetMany()
    {
        return posts.AsQueryable();
    }
}
