using PlcBase.Shared.Constants;

namespace PlcBase.Base.Error;

public class BaseException(
    int statusCode = HttpCode.INTERNAL_SERVER_ERROR,
    string message = "",
    Dictionary<string, string[]> errors = null
) : Exception(message)
{
    public int StatusCode { get; set; } = statusCode;

    public Dictionary<string, string[]> Errors { get; set; } = errors;
}
