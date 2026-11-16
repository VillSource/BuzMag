using FSH.Framework.Core.Exceptions;
using System.Net;

namespace Villsource.FSH.Exceptions;

public class ConflictException :CustomException
{
    public ConflictException()
        : base("An error occurred.", [], HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string message)
        : base(message, [], HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException, Enumerable.Empty<string>(), HttpStatusCode.Conflict)
    {
    }
}