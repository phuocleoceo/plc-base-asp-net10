using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.ConfigSetting.DTOs;
using PlcBase.Features.ConfigSetting.Services;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.ConfigSetting.Controllers;

[Route("api/config-setting")]
public class ConfigSettingController(IConfigSettingService configSettingService) : BaseController
{
    [HttpGet("")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<List<ConfigSettingDTO>>> GetAllConfigSettings()
    {
        return HttpContext.Success(await configSettingService.GetAllConfigSettings());
    }

    [HttpGet("{key}")]
    [Authorize]
    public async Task<SuccessResponse<ConfigSettingDTO>> GetConfigSettingByKey(string key)
    {
        return HttpContext.Success(await configSettingService.GetByKey(key));
    }

    [HttpPut("{key}")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<bool>> UpdateConfigSetting(
        string key,
        ConfigSettingUpdateDTO configSettingUpdateDTO
    )
    {
        if (await configSettingService.UpdateForKey(key, configSettingUpdateDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
