using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.ProjectMember.DTOs;
using PlcBase.Features.ProjectMember.Services;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.ProjectMember.Controllers;

public class ProjectMemberController(IProjectMemberService projectMemberService) : BaseController
{
    [HttpGet("/api/project/{projectId}/member")]
    [Authorize]
    public async Task<SuccessResponse<PagedList<ProjectMemberDTO>>> GetMemberForProject(
        int projectId,
        [FromQuery] ProjectMemberParams projectMemberParams
    )
    {
        return HttpContext.Success(
            await projectMemberService.GetMembersForProject(projectId, projectMemberParams)
        );
    }

    [HttpGet("/api/project/{projectId}/member/select")]
    [Authorize]
    public async Task<SuccessResponse<List<ProjectMemberSelectDTO>>> GetMemberForSelect(
        int projectId
    )
    {
        return HttpContext.Success(await projectMemberService.GetMembersForSelect(projectId));
    }

    [HttpDelete("/api/project/{projectId}/member/{projectMemberId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeleteProjectMember(int projectId, int projectMemberId)
    {
        if (await projectMemberService.DeleteProjectMember(projectId, projectMemberId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("/api/project/{projectId}/member/leave")]
    [Authorize]
    public async Task<SuccessResponse<bool>> LeaveProject(int projectId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await projectMemberService.LeaveProject(reqUser, projectId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
