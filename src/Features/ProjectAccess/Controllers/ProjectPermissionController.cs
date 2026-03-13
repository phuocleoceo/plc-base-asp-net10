using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.ProjectAccess.DTOs;
using PlcBase.Features.ProjectAccess.Services;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.ProjectAccess.Controllers;

[Route("/api/project-role/{projectRoleId}/project-permission")]
public class ProjectPermissionController(IProjectPermissionService projectPermissionService)
    : BaseController
{
    [HttpGet("")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<IEnumerable<ProjectPermissionGroupDTO>>> GetForProjectRole(
        int projectRoleId
    )
    {
        return HttpContext.Success(await projectPermissionService.GetForProjectRole(projectRoleId));
    }

    [HttpPost("")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<bool>> CreateProjectPermission(
        int projectRoleId,
        [FromBody] CreateProjectPermissionDTO createProjectPermissionDTO
    )
    {
        if (
            await projectPermissionService.CreateProjectPermission(
                projectRoleId,
                createProjectPermissionDTO
            )
        )
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("{projectPermissionKey}")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<bool>> DeleteProjectPermission(
        int projectRoleId,
        string projectPermissionKey
    )
    {
        if (
            await projectPermissionService.DeleteProjectPermission(
                projectRoleId,
                projectPermissionKey
            )
        )
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
