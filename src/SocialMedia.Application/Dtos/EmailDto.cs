using System;
using SocialMedia.Core.Entities;

namespace SocialMedia.Application.Dtos;


public class EmailDto
{
    public User User { get; set; }
    public EmailType Type { get; set; }
}

public enum EmailType
{
    Verification,
    ForgetPassword
}
