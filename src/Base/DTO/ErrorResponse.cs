using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Utilities;

namespace PlcBase.Base.DTO;

public class ErrorResponse(
    string message = "",
    int statusCode = HttpCode.BAD_REQUEST,
    Dictionary<string, string[]> errors = null
) : BaseResponse(false, statusCode)
{
    public string Message { get; set; } = message;

    public Dictionary<string, string[]> Errors { get; set; } = errors;

    public override string ToString()
    {
        return JsonUtility.Stringify(
            this,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            }
        );
    }
}
