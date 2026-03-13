using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.ProjectStatus.DTOs;
using PlcBase.Features.ProjectStatus.Services;

namespace PlcBase.Features.ProjectStatus.Controllers;

public class ProjectStatusController(IProjectStatusService projectStatusService) : BaseController
{
    [HttpGet("/api/project/{projectId}/status")]
    [Authorize]
    public async Task<SuccessResponse<List<ProjectStatusDTO>>> GetProjectStatusForProject(
        int projectId
    )
    {
        return HttpContext.Success(
            await projectStatusService.GetProjectStatusForProject(projectId)
        );
    }

    [HttpPost("/api/project/{projectId}/status")]
    [Authorize]
    public async Task<SuccessResponse<bool>> CreateProjectStatus(
        int projectId,
        [FromBody] CreateProjectStatusDTO createProjectStatusDTO
    )
    {
        if (await projectStatusService.CreateProjectStatus(projectId, createProjectStatusDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("/api/project/{projectId}/status/{projectStatusId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> UpdateProjectStatus(
        int projectId,
        int projectStatusId,
        [FromBody] UpdateProjectStatusDTO updateProjectStatusDTO
    )
    {
        if (
            await projectStatusService.UpdateProjectStatus(
                projectId,
                projectStatusId,
                updateProjectStatusDTO
            )
        )
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("/api/project/{projectId}/status/{projectStatusId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeleteProjectStatus(int projectId, int projectStatusId)
    {
        if (await projectStatusService.DeleteProjectStatus(projectId, projectStatusId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
