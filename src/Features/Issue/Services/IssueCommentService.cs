using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Issue.DTOs;
using PlcBase.Features.Issue.Entities;
using PlcBase.Shared.Constants;

namespace PlcBase.Features.Issue.Services;

public class IssueCommentService(IUnitOfWork uow, IMapper mapper) : IIssueCommentService
{
    public async Task<PagedList<IssueCommentDTO>> GetCommentsForIssue(
        int issueId,
        IssueCommentParams issueCommentParams
    )
    {
        return await uow.IssueComment.GetPagedAsync<IssueCommentDTO>(
            new QueryModel<IssueCommentEntity>()
            {
                OrderBy = p => p.OrderByDescending(c => c.CreatedAt),
                Filters = { c => c.IssueId == issueId },
                Includes = { c => c.User.UserProfile },
                PageSize = issueCommentParams.PageSize,
                PageNumber = issueCommentParams.PageNumber,
            }
        );
    }

    public async Task<bool> CreateIssueComment(
        ReqUser reqUser,
        int issueId,
        CreateIssueCommentDTO createIssueCommentDTO
    )
    {
        IssueCommentEntity issueCommentEntity = mapper.Map<IssueCommentEntity>(
            createIssueCommentDTO
        );

        issueCommentEntity.UserId = reqUser.Id;
        issueCommentEntity.IssueId = issueId;

        uow.IssueComment.Add(issueCommentEntity);
        return await uow.Save();
    }

    public async Task<bool> UpdateIssueComment(
        ReqUser reqUser,
        int issueId,
        int commentId,
        UpdateIssueCommentDTO updateIssueCommentDTO
    )
    {
        IssueCommentEntity issueCommentDb = await uow.IssueComment.GetForUpdateAndDelete(
            reqUser.Id,
            issueId,
            commentId
        );

        if (issueCommentDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "issue_comment_not_found");

        mapper.Map(updateIssueCommentDTO, issueCommentDb);
        uow.IssueComment.Update(issueCommentDb);
        return await uow.Save();
    }

    public async Task<bool> DeleteIssueComment(ReqUser reqUser, int issueId, int commentId)
    {
        IssueCommentEntity issueCommentDb = await uow.IssueComment.GetForUpdateAndDelete(
            reqUser.Id,
            issueId,
            commentId
        );

        if (issueCommentDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "issue_comment_not_found");

        uow.IssueComment.Remove(issueCommentDb);
        return await uow.Save();
    }
}
