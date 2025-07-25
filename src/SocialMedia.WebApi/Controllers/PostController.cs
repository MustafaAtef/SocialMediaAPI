using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/posts")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        public PostController(IPostService postService, ICommentService commentService, IReactService reactService)
        {
            _postService = postService;
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

        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostDto>> GetPostAsync(int postId, int commentPageSize = 10, int commentRepliesSize = 10)
        {
            return await _postService.GetPostAsync(postId, commentPageSize, commentRepliesSize);
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeleteAsync(int postId)
        {
            await _postService.DeleteAsync(postId);
            return Ok();
        }

        [HttpGet("trash")]
        public async Task<ActionResult<PagedList<UserPostsDto>>> GetAllDeletedPosts(int page = 1, int pageSize = 10)
        {
            return await _postService.GetPagedDeletedPostsAsync(page, pageSize);
        }

        [HttpPost("trash/{postId}/restore")]
        public async Task<ActionResult<UserPostsDto>> RestoreDeletedPost(int postId)
        {
            return await _postService.RestoreDeletedPostAsync(postId);
        }

        [HttpDelete("{postId}/permanent-delete")]
        public async Task<IActionResult> PermanentDeleteAsync(int postId)
        {
            await _postService.PermanentDeleteAsync(postId);
            return Ok();
        }

    }
}
