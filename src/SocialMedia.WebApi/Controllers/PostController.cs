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
        public PostController(IPostService postService, ICommentService commentService)
        {
            _commentService = commentService;
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
