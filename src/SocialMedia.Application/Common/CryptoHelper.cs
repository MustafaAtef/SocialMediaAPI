using System.Security.Cryptography;

namespace SocialMedia.Application.Common;

public static class CryptoHelper
{
    public static string GenerateRandomToken(int size = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[size];
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes);
    }
}
