using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.AccessControl.DTOs;
using PlcBase.Features.AccessControl.Services;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.AccessControl.Controllers;

public class AccessControlController(IAccessControlService accessControlService) : BaseController
{
    [HttpGet("/api/roles")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<List<RoleDTO>>> GetRoles()
    {
        return HttpContext.Success(await accessControlService.GetRoles());
    }
}
