using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.Project.DTOs;
using PlcBase.Features.Project.Services;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Project.Controllers;

public class ProjectController(IProjectService projectService) : BaseController
{
    [HttpGet]
    [Authorize]
    public async Task<SuccessResponse<PagedList<ProjectDTO>>> GetProjectsForUser(
        [FromQuery] ProjectParams projectParams
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        return HttpContext.Success(await projectService.GetProjectsForUser(reqUser, projectParams));
    }

    [HttpGet("{projectId}")]
    [Authorize]
    public async Task<SuccessResponse<ProjectDTO>> GetProjectById(int projectId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        return HttpContext.Success(await projectService.GetProjectById(reqUser, projectId));
    }

    [HttpGet("{projectId}/permission")]
    [Authorize]
    public async Task<SuccessResponse<IEnumerable<string>>> GetPermissionsInProjectForUser(
        int projectId
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        return HttpContext.Success(
            await projectService.GetPermissionsInProjectForUser(reqUser, projectId)
        );
    }

    [HttpPost]
    [Authorize]
    public async Task<SuccessResponse<bool>> CreateProject(
        [FromBody] CreateProjectDTO createProjectDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await projectService.CreateProject(reqUser, createProjectDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("{projectId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> UpdateProject(
        int projectId,
        [FromBody] UpdateProjectDTO updateProjectDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await projectService.UpdateProject(reqUser, projectId, updateProjectDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("{projectId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeleteProject(int projectId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await projectService.DeleteProject(reqUser, projectId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
