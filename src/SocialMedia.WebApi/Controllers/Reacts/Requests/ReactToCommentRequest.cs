using SocialMedia.Core.Enumerations;

namespace SocialMedia.WebApi.Controllers.Reacts.Requests;

public class ReactToCommentRequest
{
    public ReactType ReactType { get; set; }
}
