using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.Invitation.DTOs;
using PlcBase.Features.Invitation.Services;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Invitation.Controllers;

public class InvitationController(IInvitationService invitationService) : BaseController
{
    [HttpGet("/api/project/{projectId}/invitation")]
    [Authorize]
    public async Task<SuccessResponse<PagedList<SenderInvitationDTO>>> GetInvitationsForProject(
        int projectId,
        [FromQuery] SenderInvitationParams senderInvitationParams
    )
    {
        return HttpContext.Success(
            await invitationService.GetInvitationsForProject(projectId, senderInvitationParams)
        );
    }

    [HttpPost("/api/project/{projectId}/invitation")]
    [Authorize]
    public async Task<SuccessResponse<bool>> CreateInvitation(
        int projectId,
        [FromBody] CreateInvitationDTO createInvitationDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await invitationService.CreateInvitaion(reqUser, projectId, createInvitationDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("/api/project/{projectId}/invitation/{invitationId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeleteInvitation(int projectId, int invitationId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await invitationService.DeleteInvitation(reqUser, projectId, invitationId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpGet("/api/user/personal/invitation")]
    [Authorize]
    public async Task<SuccessResponse<PagedList<RecipientInvitationDTO>>> GetInvitationsForUser(
        [FromQuery] RecipientInvitationParams recipientInvitationParams
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        return HttpContext.Success(
            await invitationService.GetInvitationsForUser(reqUser, recipientInvitationParams)
        );
    }

    [HttpPut("/api/user/personal/invitation/{invitationId}/accept")]
    [Authorize]
    public async Task<SuccessResponse<bool>> AcceptInvitation(int invitationId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await invitationService.AcceptInvitation(reqUser, invitationId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("/api/user/personal/invitation/{invitationId}/decline")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeclineInvitation(int invitationId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await invitationService.DeclineInvitation(reqUser, invitationId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
