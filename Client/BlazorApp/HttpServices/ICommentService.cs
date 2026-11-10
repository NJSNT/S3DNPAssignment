using ApiContracts;

namespace BlazorApp.HttpServices;

public interface ICommentService
{
    Task<List<CommentDto>> GetCommentsForPostAsync(int postId);
    Task<CommentDto> AddCommentAsync(CreateCommentDto request);
}
