using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcCommentRepository : ICommentRepository
{
    private readonly AppContext ctx;

    public EfcCommentRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        EntityEntry<Comment> entry = await ctx.Comments.AddAsync(comment);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Comment comment)
    {
        bool exists = await ctx.Comments.AnyAsync(
            existing => existing.Id == comment.Id);

        if (!exists)
        {
            throw new EntityNotFoundException(
                $"Comment with ID '{comment.Id}' not found.");
        }

        ctx.Comments.Update(comment);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Comment? comment = await ctx.Comments.SingleOrDefaultAsync(
            existing => existing.Id == id);

        if (comment is null)
        {
            throw new EntityNotFoundException(
                $"Comment with ID '{id}' not found.");
        }

        ctx.Comments.Remove(comment);
        await ctx.SaveChangesAsync();
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        Comment? comment = await ctx.Comments.SingleOrDefaultAsync(
            existing => existing.Id == id);

        return comment ?? throw new EntityNotFoundException(
            $"Comment with ID '{id}' not found.");
    }

    public IQueryable<Comment> GetMany()
    {
        return ctx.Comments.AsQueryable();
    }
}
