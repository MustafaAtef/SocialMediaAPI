using System;

namespace SocialMedia.Core.Exceptions;

public class UniqueException : Exception
{
    public UniqueException()
    {
    }

    public UniqueException(string message) : base(message)
    {
    }

}
