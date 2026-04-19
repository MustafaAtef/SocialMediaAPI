using System;

namespace SocialMedia.Application.Options;

public class ExpireDurationsOptions
{
    public int EmailVerificationTokenExpiryMinutes { get; set; }
    public int PasswordResetTokenExpiryMinutes { get; set; }
}
