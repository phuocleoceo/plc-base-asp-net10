using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.ProjectAccess.DTOs;
using PlcBase.Features.ProjectAccess.Services;

namespace PlcBase.Features.ProjectAccess.Controllers;

[Route("api/member-role")]
public class MemberRoleController(IMemberRoleService memberRoleService) : BaseController
{
    [HttpGet("{projectMemberId}")]
    [Authorize]
    public async Task<SuccessResponse<List<MemberRoleDTO>>> GetProjectRoleForMember(
        int projectMemberId
    )
    {
        return HttpContext.Success(
            await memberRoleService.GetProjectRoleForMember(projectMemberId)
        );
    }

    [HttpPost("")]
    [Authorize]
    public async Task<SuccessResponse<bool>> CreateMemberRole(
        [FromBody] CreateMemberRoleDTO createMemberRoleDTO
    )
    {
        if (await memberRoleService.CreateMemberRole(createMemberRoleDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeleteMemberRole(
        int projectMemberId,
        int projectRoleId
    )
    {
        if (await memberRoleService.DeleteMemberRole(projectMemberId, projectRoleId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
