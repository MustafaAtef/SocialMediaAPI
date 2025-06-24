using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Dtos;

public class ReactToPostDto
{
    public int PostId { get; set; }
    public ReactType ReactType { get; set; }
}

public class ReactToCommentDto
{
    public int PostId { get; set; }
    public int CommentId { get; set; }
    public ReactType ReactType { get; set; }
}
public class PostReactDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public UserDto ReactedBy { get; set; }
    public ReactType ReactType { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommentReactDto
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public UserDto ReactedBy { get; set; }
    public ReactType ReactType { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}


