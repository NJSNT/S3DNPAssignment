using Entities;
using RepositoryContracts;
using System.Text.Json;

namespace FileRepositories;

public class UserFileRepository : IUserRepository
{
    private readonly string filePath = "users.json";

    public UserFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public async Task<User> AddAsync(User user)
    {
        string usersAsJson = await File.ReadAllTextAsync(filePath);

        List<User> users =
            JsonSerializer.Deserialize<List<User>>(usersAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        int maxId = users.Count > 0
            ? users.Max(u => u.Id)
            : 0;

        user.Id = maxId + 1;
        users.Add(user);

        usersAsJson = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(filePath, usersAsJson);

        return user;
    }

    public async Task UpdateAsync(User user)
    {
        string usersAsJson = await File.ReadAllTextAsync(filePath);

        List<User> users =
            JsonSerializer.Deserialize<List<User>>(usersAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        User? existingUser =
            users.SingleOrDefault(u => u.Id == user.Id);

        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);
        users.Add(user);

        usersAsJson = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(filePath, usersAsJson);
    }

    public async Task DeleteAsync(int id)
    {
        string usersAsJson = await File.ReadAllTextAsync(filePath);

        List<User> users =
            JsonSerializer.Deserialize<List<User>>(usersAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        User? userToRemove =
            users.SingleOrDefault(u => u.Id == id);

        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);

        usersAsJson = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(filePath, usersAsJson);
    }

    public async Task<User> GetSingleAsync(int id)
    {
        string usersAsJson = await File.ReadAllTextAsync(filePath);

        List<User> users =
            JsonSerializer.Deserialize<List<User>>(usersAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        User? user = users.SingleOrDefault(u => u.Id == id);

        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        return user;
    }

    public IQueryable<User> GetMany()
    {
        // Keep the synchronous interface while letting file errors reach the CLI directly.
        string usersAsJson = File.ReadAllTextAsync(filePath).GetAwaiter().GetResult();

        List<User> users =
            JsonSerializer.Deserialize<List<User>>(usersAsJson)
            ?? throw new JsonException($"File '{filePath}' must contain a JSON array, not null.");

        return users.AsQueryable();
    }
}
