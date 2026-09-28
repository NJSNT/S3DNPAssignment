using RepositoryContracts;
using Entities;
using System.Text.Json;

namespace CLI.UI;

public class CliApp
{
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public CliApp(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }

    public async Task StartAsync()
    {
        Console.WriteLine("Welcome to the Forum CLI!");

        while (true)
        {
            Console.WriteLine("\n1. Create user");
            Console.WriteLine("2. Create post");
            Console.WriteLine("3. Add comment");
            Console.WriteLine("4. View posts");
            Console.WriteLine("5. View specific post");
            Console.WriteLine("0. Exit");
            Console.Write("Select: ");

            string? choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "0":
                    case null:
                        return;

                    case "1":
                        await CreateUserAsync();
                        break;

                    case "2":
                        await CreatePostAsync();
                        break;

                    case "3":
                        await AddCommentAsync();
                        break;

                    case "4":
                        ShowPosts();
                        break;

                    case "5":
                        await ShowPostAsync();
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (JsonException exception)
            {
                Console.WriteLine($"Invalid data file. Check the JSON content: {exception.Message}");
            }
            catch (IOException exception)
            {
                Console.WriteLine($"Could not read or save the data file: {exception.Message}");
            }
            catch (UnauthorizedAccessException exception)
            {
                Console.WriteLine($"Cannot access the data file. Check file permissions: {exception.Message}");
            }
        }
    }
    
    private async Task CreateUserAsync()
    {
        Console.Write("Username: ");
        string? username = Console.ReadLine();

        Console.Write("Password: ");
        string? password = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("Username must not be empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Password must not be empty.");
            return;
        }

        bool usernameExists = userRepository.GetMany()
            .Any(u => u.UserName == username);

        if (usernameExists)
        {
            Console.WriteLine("Username is already taken.");
            return;
        }

        User user = new User
        {
            UserName = username,
            Password = password
        };

        User created = await userRepository.AddAsync(user);

        Console.WriteLine($"Created user with ID: {created.Id}");
    }

    private async Task CreatePostAsync()
    {
        Console.Write("Title: ");
        string? title = Console.ReadLine();

        Console.Write("Body: ");
        string? body = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Title must not be empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Body must not be empty.");
            return;
        }

        Console.Write("User ID: ");
        if (!int.TryParse(Console.ReadLine(), out int userId) || userId <= 0)
        {
            Console.WriteLine("User ID must be a positive whole number.");
            return;
        }

        bool userExists = userRepository.GetMany()
            .Any(u => u.Id == userId);

        if (!userExists)
        {
            Console.WriteLine("User not found.");
            return;
        }

        Post post = new Post
        {
            Title = title,
            Body = body,
            UserId = userId
        };

        Post created = await postRepository.AddAsync(post);
        Console.WriteLine($"Created post with ID: {created.Id}");
    }

    private async Task AddCommentAsync()
    {
        Console.Write("Comment body: ");
        string? body = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(body))
        {
            Console.WriteLine("Comment must not be empty.");
            return;
        }

        Console.Write("User ID: ");
        if (!int.TryParse(Console.ReadLine(), out int userId) || userId <= 0)
        {
            Console.WriteLine("Invalid user ID.");
            return;
        }

        Console.Write("Post ID: ");
        if (!int.TryParse(Console.ReadLine(), out int postId) || postId <= 0)
        {
            Console.WriteLine("Invalid post ID.");
            return;
        }

        if (!userRepository.GetMany().Any(u => u.Id == userId))
        {
            Console.WriteLine("User not found.");
            return;
        }

        if (!postRepository.GetMany().Any(p => p.Id == postId))
        {
            Console.WriteLine("Post not found.");
            return;
        }

        Comment comment = new Comment
        {
            Body = body,
            UserId = userId,
            PostId = postId
        };

        Comment created = await commentRepository.AddAsync(comment);
        Console.WriteLine($"Created comment with ID: {created.Id}");
    }

    private void ShowPosts()
    {
        var posts = postRepository.GetMany();

        if (!posts.Any())
        {
            Console.WriteLine("No posts yet.");
            return;
        }

        foreach (Post post in posts)
        {
            Console.WriteLine($"{post.Id} - {post.Title}");
        }
    }

    private async Task ShowPostAsync()
    {
        Console.Write("Post ID: ");
        if (!int.TryParse(Console.ReadLine(), out int postId) || postId <= 0)
        {
            Console.WriteLine("Invalid post ID.");
            return;
        }

        try
        {
            Post post = await postRepository.GetSingleAsync(postId);

            Console.WriteLine($"Title: {post.Title}");
            Console.WriteLine($"Author ID: {post.UserId}");
            Console.WriteLine($"Body: {post.Body}");
            Console.WriteLine("Comments:");

            var comments = commentRepository.GetMany()
                .Where(c => c.PostId == postId);

            if (!comments.Any())
            {
                Console.WriteLine("No comments yet.");
            }

            foreach (Comment comment in comments)
            {
                Console.WriteLine($"User {comment.UserId}: {comment.Body}");
            }
        }
        catch (InvalidOperationException exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
    
}
