using System;

namespace SocialMedia.Infrastructure.Email;

public class EmailOptions
{
    public string Smtp { get; set; }
    public int Port { get; set; }
    public string From { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string BaseUrl { get; set; }
    public string EmailVerificationPath { get; set; }
    public string PasswordResetPath { get; set; }

    public string BuildVerificationUrl(string token) =>
        BaseUrl + EmailVerificationPath.Replace("{token}", token);

    public string BuildPasswordResetUrl(string token) =>
        BaseUrl + PasswordResetPath.Replace("{token}", token);
}