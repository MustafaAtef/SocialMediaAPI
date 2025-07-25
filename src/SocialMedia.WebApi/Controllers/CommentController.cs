using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [HttpPost("{postId}/comments")]
        [Authorize]
        public async Task<ActionResult<CommentWithoutRepliesDto>> CreateAsync(int postId, CreateCommentDto createCommentDto)
        {
            createCommentDto.PostId = postId;
            return await _commentService.CreateAsync(createCommentDto);
        }

        [HttpPost("{postId}/comments/{parentCommentId}")]
        [Authorize]
        public async Task<ActionResult<CommentWithoutRepliesDto>> ReplyAsync(int postId, int parentCommentId, ReplyCommentDto replyCommentDto)
        {
            replyCommentDto.PostId = postId;
            replyCommentDto.ParentCommentId = parentCommentId;
            return await _commentService.ReplyAsync(replyCommentDto);
        }

        [HttpPut("{postId}/comments/{commentId}")]
        [Authorize]
        public async Task<ActionResult<CommentDto>> UpdateAsync(int postId, int commentId, UpdateCommentDto updateCommentDto)
        {
            updateCommentDto.PostId = postId;
            updateCommentDto.CommentId = commentId;
            return await _commentService.UpdateAsync(updateCommentDto);
        }
        [HttpGet("{postId}/comments")]
        public async Task<ActionResult<PagedList<CommentDto>>> GetAllPostComments(int postId, int page = 1, int pageSize = 10, int repliesSize = 10)
        {
            return await _commentService.GetPagedCommentsAsync(postId, page, pageSize, repliesSize);
        }

        [HttpGet("{postId}/comments/{commentId}/replies")]
        public async Task<ActionResult<PagedList<CommentWithoutRepliesDto>>> GetAllCommentReplies(int commentId, int page = 1, int pageSize = 10)
        {
            return await _commentService.GetPagedRepliesAsync(commentId, page, pageSize);
        }

        [HttpDelete("{postId}/comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAsync(int postId, int commentId)
        {
            await _commentService.DeleteAsync(postId, commentId);
            return Ok();
        }
    }

}
