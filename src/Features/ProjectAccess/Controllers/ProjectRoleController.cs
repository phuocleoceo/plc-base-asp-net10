using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.ProjectAccess.DTOs;
using PlcBase.Features.ProjectAccess.Services;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.ProjectAccess.Controllers;

[Route("/api/project-role")]
public class ProjectRoleController(IProjectRoleService projectRoleService) : BaseController
{
    [HttpGet("")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<PagedList<ProjectRoleDTO>>> GetProjectRoles(
        [FromQuery] ProjectRoleParams roleParams
    )
    {
        return HttpContext.Success(await projectRoleService.GetProjectRoles(roleParams));
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<SuccessResponse<List<ProjectRoleDTO>>> GetAllProjectRoles()
    {
        return HttpContext.Success(await projectRoleService.GetAllProjectRoles());
    }

    [HttpGet("{projectRoleId}")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<ProjectRoleDTO>> GetProjectRoleById(int projectRoleId)
    {
        return HttpContext.Success(await projectRoleService.GetProjectRoleById(projectRoleId));
    }

    [HttpPost("")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<bool>> CreateProjectRole(
        [FromBody] CreateProjectRoleDTO createProjectRoleDTO
    )
    {
        if (await projectRoleService.CreateProjectRole(createProjectRoleDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("{projectRoleId}")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<bool>> UpdateProjectRole(
        int projectRoleId,
        [FromBody] UpdateProjectRoleDTO updateProjectRoleDTO
    )
    {
        if (await projectRoleService.UpdateProjectRole(projectRoleId, updateProjectRoleDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("{projectRoleId}")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<bool>> DeleteProjectRole(int projectRoleId)
    {
        if (await projectRoleService.DeleteProjectRole(projectRoleId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
