using SocialMedia.Core.Enumerations;

namespace SocialMedia.WebApi.Controllers.Reacts.Requests;

public class ReactToPostRequest
{
    public ReactType ReactType { get; set; }
}