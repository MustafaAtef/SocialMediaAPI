using System;

namespace SocialMedia.Infrastructure.Email;

public class EmailOptions
{
    public string Smtp { get; set; }
    public int Port { get; set; }
    public string From { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string EmailVerificationUrl { get; set; }
    public string PasswordResetUrl { get; set; }

}