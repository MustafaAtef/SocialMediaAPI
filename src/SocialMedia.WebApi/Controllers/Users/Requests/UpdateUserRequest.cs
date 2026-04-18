
namespace SocialMedia.WebApi.Controllers.Users.Requests;

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public IFormFile? Avatar { get; set; }
}