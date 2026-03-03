namespace SocialMedia.Core.Errors;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("NullValue", "The value is null.");
}
