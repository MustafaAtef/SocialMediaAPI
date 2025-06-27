using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/posts")]
    [ApiController]
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
        public async Task<ActionResult<PostDto>> GetPostAsync(int postId, int commentPageSize = 10, int commentRepliesSize = 10)
        {
            return await _postService.GetPostAsync(postId, commentPageSize, commentRepliesSize);
        }
    }
}
