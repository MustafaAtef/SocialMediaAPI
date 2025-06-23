using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Dtos;

public class ReactToPostDto
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public ReactType ReactType { get; set; }
}

public class ReactToCommentDto
{
    public int CommentId { get; set; }
    public int UserId { get; set; }
    public ReactType ReactType { get; set; }
}
public class ReactDto
{

}
