using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class ReactController : ControllerBase
    {
        private readonly IReactService _reactService;
        public ReactController(IReactService reactService)
        {
            _reactService = reactService;
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
