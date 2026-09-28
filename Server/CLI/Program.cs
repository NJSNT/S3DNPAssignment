namespace CLI;
using CLI.UI;
using FileRepositories;
using RepositoryContracts;
    
public class Program
{
    
    static async Task Main(string[] args)
    {
        try
        {
            IUserRepository userRepository = new UserFileRepository();
            IPostRepository postRepository = new PostFileRepository();
            ICommentRepository commentRepository = new CommentFileRepository();

            CliApp app = new CliApp(
                userRepository,
                postRepository,
                commentRepository
            );
            await app.StartAsync();
        }
        catch (IOException exception)
        {
            Console.WriteLine($"Could not initialize the data files: {exception.Message}");
            Environment.ExitCode = 1;
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.WriteLine($"Cannot access the data files. Check file permissions: {exception.Message}");
            Environment.ExitCode = 1;
        }
    }
}
