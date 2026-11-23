using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepositoryContracts;

namespace EfcRepositories;

public class EfcUserRepository : IUserRepository
{
    private readonly AppContext ctx;

    public EfcUserRepository(AppContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<User> AddAsync(User user)
    {
        EntityEntry<User> entry = await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(User user)
    {
        bool exists = await ctx.Users.AnyAsync(
            existing => existing.Id == user.Id);

        if (!exists)
        {
            throw new EntityNotFoundException(
                $"User with ID '{user.Id}' not found.");
        }

        ctx.Users.Update(user);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        User? user = await ctx.Users.SingleOrDefaultAsync(
            existing => existing.Id == id);

        if (user is null)
        {
            throw new EntityNotFoundException(
                $"User with ID '{id}' not found.");
        }

        ctx.Users.Remove(user);
        await ctx.SaveChangesAsync();
    }

    public async Task<User> GetSingleAsync(int id)
    {
        User? user = await ctx.Users.SingleOrDefaultAsync(
            existing => existing.Id == id);

        return user ?? throw new EntityNotFoundException(
            $"User with ID '{id}' not found.");
    }

    public IQueryable<User> GetMany()
    {
        return ctx.Users.AsQueryable();
    }
}
