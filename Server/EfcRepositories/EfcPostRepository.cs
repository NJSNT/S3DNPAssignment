using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcPostRepository : IPostRepository
{
    private readonly AppContext ctx;

    public EfcPostRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Post> AddAsync(Post post)
    {
        EntityEntry<Post> entry = await ctx.Posts.AddAsync(post);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Post post)
    {
        bool exists = await ctx.Posts.AnyAsync(
            existing => existing.Id == post.Id);

        if (!exists)
        {
            throw new EntityNotFoundException(
                $"Post with ID '{post.Id}' not found.");
        }

        ctx.Posts.Update(post);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Post? post = await ctx.Posts.SingleOrDefaultAsync(
            existing => existing.Id == id);

        if (post is null)
        {
            throw new EntityNotFoundException(
                $"Post with ID '{id}' not found.");
        }

        ctx.Posts.Remove(post);
        await ctx.SaveChangesAsync();
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        Post? post = await ctx.Posts.SingleOrDefaultAsync(
            existing => existing.Id == id);

        return post ?? throw new EntityNotFoundException(
            $"Post with ID '{id}' not found.");
    }

    public IQueryable<Post> GetMany()
    {
        return ctx.Posts.AsQueryable();
    }
}