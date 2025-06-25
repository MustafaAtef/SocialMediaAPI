using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [HttpPost("{postId}/comment")]
        public async Task<ActionResult<CreatedCommentDto>> CreateAsync(int postId, CreateCommentDto createCommentDto)
        {
            createCommentDto.PostId = postId;
            return await _commentService.CreateAsync(createCommentDto);
        }

        [HttpPost("{postId}/comment/{parentCommentId}")]
        public async Task<ActionResult<CreatedCommentDto>> ReplyAsync(int postId, int parentCommentId, ReplyCommentDto replyCommentDto)
        {
            replyCommentDto.PostId = postId;
            replyCommentDto.ParentCommentId = parentCommentId;
            return await _commentService.ReplyAsync(replyCommentDto);
        }

        [HttpPut("{postId}/comment/{commentId}")]
        public async Task<ActionResult<CommentDto>> UpdateAsync(int postId, int commentId, UpdateCommentDto updateCommentDto)
        {
            updateCommentDto.PostId = postId;
            updateCommentDto.CommentId = commentId;
            return await _commentService.UpdateAsync(updateCommentDto);
        }
    }
}
