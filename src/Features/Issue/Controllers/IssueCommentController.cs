using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.Issue.DTOs;
using PlcBase.Features.Issue.Services;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Issue.Controllers;

public class IssueCommentController(IIssueCommentService issueCommentService) : BaseController
{
    [HttpGet("/api/issue/{issueId}/comment")]
    [Authorize]
    public async Task<SuccessResponse<PagedList<IssueCommentDTO>>> GetCommentsForIssue(
        int issueId,
        [FromQuery] IssueCommentParams issueCommentParams
    )
    {
        return HttpContext.Success(
            await issueCommentService.GetCommentsForIssue(issueId, issueCommentParams)
        );
    }

    [HttpPost("/api/issue/{issueId}/comment")]
    [Authorize]
    public async Task<SuccessResponse<bool>> CreateIssueComment(
        int issueId,
        [FromBody] CreateIssueCommentDTO createIssueCommentDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await issueCommentService.CreateIssueComment(reqUser, issueId, createIssueCommentDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("/api/issue/{issueId}/comment/{commentId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> UpdateIssueComment(
        int issueId,
        int commentId,
        [FromBody] UpdateIssueCommentDTO updateIssueCommentDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (
            await issueCommentService.UpdateIssueComment(
                reqUser,
                issueId,
                commentId,
                updateIssueCommentDTO
            )
        )
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("/api/issue/{issueId}/comment/{commentId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeleteIssueComment(int issueId, int commentId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await issueCommentService.DeleteIssueComment(reqUser, issueId, commentId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
