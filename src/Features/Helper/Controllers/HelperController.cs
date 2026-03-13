using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.Helper.DTOs;
using PlcBase.Shared.Helpers;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Helper.Controllers;

[Route("api")]
public class HelperController(IS3Helper s3Helper) : BaseController
{
    [HttpPost("upload-file")]
    [Authorize]
    public async Task<SuccessResponse<string>> S3Upload(IFormFile file, string prefix = "")
    {
        return HttpContext.Success(await s3Helper.UploadFile(file.GetS3FileUpload(prefix)));
    }

    [HttpPost("presigned-upload-url")]
    [Authorize]
    public async Task<SuccessResponse<S3PresignedUrlResponse>> GetS3PresignedUploadUrl(
        S3PresignedUrlRequest request
    )
    {
        return HttpContext.Success(await s3Helper.GetPresignedUploadUrl(request));
    }
}
