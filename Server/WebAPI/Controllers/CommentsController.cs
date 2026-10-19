using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepository;
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;

    public CommentsController(
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IPostRepository postRepository)
    {
        this.commentRepository = commentRepository;
        this.userRepository = userRepository;
        this.postRepository = postRepository;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body must not be empty.");
        }

        if (request.UserId <= 0 || request.PostId <= 0)
        {
            return BadRequest("User ID and post ID must be positive whole numbers.");
        }

        await userRepository.GetSingleAsync(request.UserId);
        await postRepository.GetSingleAsync(request.PostId);

        Comment comment = new Comment
        {
            Body = request.Body,
            UserId = request.UserId,
            PostId = request.PostId
        };

        Comment created = await commentRepository.AddAsync(comment);
        CommentDto dto = ToDto(created);

        return CreatedAtAction(nameof(GetComment), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetComment([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest("Comment ID must be a positive whole number.");
        }

        Comment comment = await commentRepository.GetSingleAsync(id);
        return Ok(ToDto(comment));
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments(
        [FromQuery] int? userId = null,
        [FromQuery] int? postId = null)
    {
        if ((userId.HasValue && userId.Value <= 0) ||
            (postId.HasValue && postId.Value <= 0))
        {
            return BadRequest("User ID and post ID must be positive whole numbers.");
        }

        IQueryable<Comment> comments = commentRepository.GetMany();

        if (userId.HasValue)
        {
            comments = comments.Where(comment => comment.UserId == userId.Value);
        }

        if (postId.HasValue)
        {
            comments = comments.Where(comment => comment.PostId == postId.Value);
        }

        List<CommentDto> dtos = comments.Select(comment => new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            UserId = comment.UserId,
            PostId = comment.PostId
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("/Posts/{postId:int}/Comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost(
        [FromRoute] int postId)
    {
        if (postId <= 0)
        {
            return BadRequest("Post ID must be a positive whole number.");
        }

        await postRepository.GetSingleAsync(postId);
        return GetComments(postId: postId);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateComment(
        [FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        if (id <= 0)
        {
            return BadRequest("Comment ID must be a positive whole number.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body must not be empty.");
        }

        Comment comment = await commentRepository.GetSingleAsync(id);
        comment.Body = request.Body;

        await commentRepository.UpdateAsync(comment);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        if (id <= 0)
        {
            return BadRequest("Comment ID must be a positive whole number.");
        }

        await commentRepository.DeleteAsync(id);
        return NoContent();
    }

    private static CommentDto ToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            UserId = comment.UserId,
            PostId = comment.PostId
        };
    }
}
