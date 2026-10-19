using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public PostsController(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body must not be empty.");
        }

        if (request.UserId <= 0)
        {
            return BadRequest("User ID must be a positive whole number.");
        }

        await userRepository.GetSingleAsync(request.UserId);

        Post post = new Post
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId
        };

        Post created = await postRepository.AddAsync(post);
        PostDto dto = ToDto(created);

        return CreatedAtAction(nameof(GetPost), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetPost([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest("Post ID must be a positive whole number.");
        }

        Post post = await postRepository.GetSingleAsync(id);
        return Ok(ToDto(post));
    }

    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> GetPosts(
        [FromQuery] string? title = null,
        [FromQuery] int? userId = null)
    {
        if (userId.HasValue && userId.Value <= 0)
        {
            return BadRequest("User ID must be a positive whole number.");
        }

        IQueryable<Post> posts = postRepository.GetMany();

        if (!string.IsNullOrWhiteSpace(title))
        {
            posts = posts.Where(post => post.Title.Contains(
                title, StringComparison.OrdinalIgnoreCase));
        }

        if (userId.HasValue)
        {
            posts = posts.Where(post => post.UserId == userId.Value);
        }

        List<PostDto> dtos = posts.Select(post => new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId
        }).ToList();

        return Ok(dtos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdatePost(
        [FromRoute] int id, [FromBody] UpdatePostDto request)
    {
        if (id <= 0)
        {
            return BadRequest("Post ID must be a positive whole number.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body must not be empty.");
        }

        Post post = await postRepository.GetSingleAsync(id);
        post.Title = request.Title;
        post.Body = request.Body;

        await postRepository.UpdateAsync(post);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest("Post ID must be a positive whole number.");
        }

        await postRepository.GetSingleAsync(id);

        if (commentRepository.GetMany().Any(comment => comment.PostId == id))
        {
            return Conflict("Post has comments. Delete its comments before deleting the post.");
        }

        await postRepository.DeleteAsync(id);
        return NoContent();
    }

    private static PostDto ToDto(Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId
        };
    }
}
