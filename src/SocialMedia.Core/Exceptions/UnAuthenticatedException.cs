using System;

namespace SocialMedia.Core.Exceptions;

public class UnAuthenticatedException : Exception
{
    public UnAuthenticatedException()
    {
    }

    public UnAuthenticatedException(string message) : base(message)
    {
    }

}
