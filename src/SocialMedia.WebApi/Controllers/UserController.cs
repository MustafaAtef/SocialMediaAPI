using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;

        public UserController(IUserService userService, IPostService postService)
        {
            _userService = userService;
            _postService = postService;
        }

        [HttpPost("{userId}/follow")]
        public async Task<ActionResult> Follow(int userId)
        {
            await _userService.FollowAsync(userId);
            return Ok();
        }

        [HttpDelete("{userId}/follow")]
        public async Task<ActionResult> UnFollow(int userId)
        {
            await _userService.UnFollowAsync(userId);
            return Ok();
        }

        [HttpGet("{userId}/followers")]
        public async Task<ActionResult<PagedList<UserDto>>> GetFollowers(int userId, int page = 1, int pageSize = 100)
        {
            return await _userService.GetPagedFollowersAsync(userId, page, pageSize);
        }

        [HttpGet("{userId}/followings")]
        public async Task<ActionResult<PagedList<UserDto>>> GetFollowings(int userId, int page = 1, int pageSize = 100)
        {
            return await _userService.GetPagedFollowingsAsync(userId, page, pageSize);
        }

        [HttpGet("{userId}/posts")]
        public async Task<ActionResult<PagedList<UserPostsDto>>> GetAllPosts(int userId, int page = 1, int pageSize = 10)
        {
            return await _postService.GetPagedPostsAsync(userId, page, pageSize);
        }

        [HttpGet("/messages")]
        public async Task<ActionResult<ICollection<GroupMessagesDto>>> GetAllGroupsMessages(int olderMessagesSize = 25)
        {
            var messages = await _userService.GetAllGroupMessagesAsync(olderMessagesSize);
            return Ok(messages);
        }

        [HttpGet("/messages/groups/{groupId}")]
        public async Task<ActionResult<ICollection<GroupMessagesDto>>> GetGroupMessages(Guid groupId, int? lastMessageId, int olderMessagesSize = 25)
        {
            var messages = await _userService.GetPagedGroupMessagesAsync(groupId, lastMessageId, olderMessagesSize);
            return Ok(messages);
        }

    }
}
