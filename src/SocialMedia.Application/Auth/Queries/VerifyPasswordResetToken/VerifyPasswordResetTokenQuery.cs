using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Auth.Queries.VerifyPasswordResetToken;

public sealed record VerifyPasswordResetTokenQuery(string Token) : IQuery<bool>;