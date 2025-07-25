using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/posts")]
    [ApiController]
    [Authorize]
    public class ReactController : ControllerBase
    {
        private readonly IReactService _reactService;
        public ReactController(IReactService reactService)
        {
            _reactService = reactService;
        }
        [HttpPost("{postId}/reacts")]
        public async Task<ActionResult<PostReactDto>> ReactToPostAsync(int postId, [FromBody] ReactToPostDto reactToPostDto)
        {
            reactToPostDto.PostId = postId;
            return await _reactService.ReactToPostAsync(reactToPostDto);
        }

        [HttpPost("{postId}/comments/{commentId}/reacts")]
        public async Task<ActionResult<CommentReactDto>> ReactToCommentAsync(int postId, int commentId, [FromBody] ReactToCommentDto reactToCommentDto)
        {
            reactToCommentDto.PostId = postId;
            reactToCommentDto.CommentId = commentId;
            return await _reactService.ReactToCommentAsync(reactToCommentDto);
        }

        [HttpDelete("{postId}/reacts")]
        public async Task<IActionResult> RemovePostReactAsync(int postId)
        {
            await _reactService.RemovePostReactAsync(postId);
            return Ok();
        }

        [HttpDelete("{postId}/comments/{commentId}/reacts")]
        public async Task<IActionResult> RemoveCommentReactAsync(int commentId)
        {
            await _reactService.RemoveCommentReactAsync(commentId);
            return Ok();
        }

        [HttpGet("{postId}/reacts")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<PostReactDto>>> GetPostReactsAsync(int postId, int page = 1, int pageSize = 100)
        {
            return await _reactService.GetPagedPostReactsAsync(postId, page, pageSize);
        }

        [HttpGet("{postId}/comments/{commentId}/reacts")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<CommentReactDto>>> GetCommentReactsAsync(int commentId, int page = 1, int pageSize = 100)
        {
            return await _reactService.GetPagedcommentReactsAsync(commentId, page, pageSize);
        }
    }
}
