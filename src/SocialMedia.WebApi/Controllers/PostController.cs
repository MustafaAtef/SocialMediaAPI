using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IReactService _reactService;
        public PostController(IPostService postService, ICommentService commentService, IReactService reactService)
        {
            _postService = postService;
            _commentService = commentService;
            _reactService = reactService;
        }

        [HttpPost]
        public async Task<ActionResult<PostDto>> CreateAsync([FromForm] CreatePostDto createPostDto)
        {
            return await _postService.CreateAsync(createPostDto);
        }

        [HttpPut("{postId}")]
        public async Task<ActionResult<PostDto>> UpdateAsync(int postId, [FromForm] UpdatePostDto updatePostDto)
        {
            updatePostDto.PostId = postId;
            return await _postService.UpdateAsync(updatePostDto);
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

        [HttpPost("{postId}/react")]
        public async Task<ActionResult<PostReactDto>> ReactToPostAsync(int postId, [FromBody] ReactToPostDto reactToPostDto)
        {
            reactToPostDto.PostId = postId;
            return await _reactService.ReactToPostAsync(reactToPostDto);
        }

        [HttpPost("{postId}/comment/{commentId}/react")]
        public async Task<ActionResult<CommentReactDto>> ReactToCommentAsync(int postId, int commentId, [FromBody] ReactToCommentDto reactToCommentDto)
        {
            reactToCommentDto.PostId = postId;
            reactToCommentDto.CommentId = commentId;
            return await _reactService.ReactToCommentAsync(reactToCommentDto);
        }

        [HttpDelete("{postId}/react")]
        public async Task<IActionResult> RemovePostReactAsync(int postId)
        {
            await _reactService.RemovePostReactAsync(postId);
            return Ok();
        }

        [HttpDelete("{postId}/comment/{commentId}/react")]
        public async Task<IActionResult> RemoveCommentReactAsync(int postId, int commentId)
        {
            await _reactService.RemoveCommentReactAsync(commentId);
            return Ok();
        }

    }
}
