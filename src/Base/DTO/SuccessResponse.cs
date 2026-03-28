using PlcBase.Shared.Constants;
using PlcBase.Shared.Utilities;

namespace PlcBase.Base.DTO;

public class SuccessResponse<T> : BaseResponse
{
    public T Data { get; set; }

    public SuccessResponse()
    {
        Data = default;
    }

    public SuccessResponse(T data, int statusCode = HttpCode.OK)
        : base(true, statusCode)
    {
        Data = data;
    }

    public override string ToString()
    {
        return JsonUtility.Serialize(this, true);
    }
}
