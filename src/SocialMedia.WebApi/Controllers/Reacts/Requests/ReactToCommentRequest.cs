using SocialMedia.Application.CustomValidations;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.WebApi.Controllers.Reacts.Requests;

public class ReactToCommentRequest
{
    [EnumValue(typeof(ReactType), true, ErrorMessage = "React type is required and valid values are from (1 - 5).")]
    public ReactType ReactType { get; set; }
}
