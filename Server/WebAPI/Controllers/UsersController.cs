using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public UsersController(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest("Username must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Password must not be empty.");
        }

        bool usernameExists = userRepository.GetMany()
            .Any(user => user.UserName == request.UserName);

        if (usernameExists)
        {
            return BadRequest("Username is already taken.");
        }

        User user = new User
        {
            UserName = request.UserName,
            Password = request.Password
        };

        User created = await userRepository.AddAsync(user);

        UserDto dto = new UserDto
        {
            Id = created.Id,
            UserName = created.UserName
        };

        return CreatedAtAction(nameof(GetUser), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateUser(
        [FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        if (id <= 0)
        {
            return BadRequest("User ID must be a positive whole number.");
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest("Username must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Password must not be empty.");
        }

        User user = await userRepository.GetSingleAsync(id);

        bool usernameExists = userRepository.GetMany()
            .Any(other => other.Id != id && other.UserName == request.UserName);

        if (usernameExists)
        {
            return BadRequest("Username is already taken.");
        }

        user.UserName = request.UserName;
        user.Password = request.Password;

        await userRepository.UpdateAsync(user);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest("User ID must be a positive whole number.");
        }

        await userRepository.GetSingleAsync(id);

        if (postRepository.GetMany().Any(post => post.UserId == id) ||
            commentRepository.GetMany().Any(comment => comment.UserId == id))
        {
            return Conflict("User has posts or comments. Delete them before deleting the user.");
        }

        await userRepository.DeleteAsync(id);

        return NoContent();
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers([FromQuery] string? userName = null)
    {
        IQueryable<User> users = userRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            users = users.Where(user => user.UserName.Contains(
                userName, StringComparison.OrdinalIgnoreCase));
        }

        List<UserDto> dtos = users.Select(user => new UserDto
        {
            Id = user.Id,
            UserName = user.UserName
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest("User ID must be a positive whole number.");
        }

        User user = await userRepository.GetSingleAsync(id);

        UserDto dto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName
        };

        return Ok(dto);
    }
}
