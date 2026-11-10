using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository userRepository;

    public AuthController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpPost("login")]
    public ActionResult<UserDto> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized("Invalid username or password.");
        }

        User? user = userRepository.GetMany()
            .SingleOrDefault(user => user.UserName == request.UserName);

        if (user is null || user.Password != request.Password)
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName
        });
    }
}
