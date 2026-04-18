namespace SocialMedia.Core.Errors;

public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static readonly Error None = new(ErrorType.None, string.Empty, string.Empty);
    public static readonly Error NullValue = new(ErrorType.Validation, "NullValue", "The value is null.");
    public static Error Validation(string field, string message) => new(ErrorType.Validation, field, message);
}
