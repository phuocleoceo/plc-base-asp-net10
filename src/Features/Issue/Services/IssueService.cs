using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Issue.DTOs;
using PlcBase.Features.Issue.Entities;
using PlcBase.Features.Sprint.Entities;
using PlcBase.Shared.Constants;

namespace PlcBase.Features.Issue.Services;

public class IssueService(IUnitOfWork uow, IMapper mapper) : IIssueService
{
    #region Board
    public async Task<IEnumerable<IssueBoardGroupDTO>> GetIssuesForBoard(
        int projectId,
        int sprintId,
        IssueBoardParams issueParams
    )
    {
        QueryModel<IssueEntity> issueQuery = new QueryModel<IssueEntity>()
        {
            OrderBy = c => c.OrderBy(i => i.ProjectStatusIndex),
            Filters =
            {
                i =>
                    i.ProjectId == projectId
                    && i.DeletedAt == null
                    && i.SprintId == sprintId
                    && i.BacklogIndex == null,
            },
            Includes = { i => i.Assignee.UserProfile },
        };

        if (!String.IsNullOrEmpty(issueParams.Assignees))
        {
            IEnumerable<int> assignees = issueParams
                .Assignees.Split(",")
                .Select(c => Convert.ToInt32(c));
            issueQuery.Filters.Add(i => assignees.Contains(i.AssigneeId.Value));
        }

        if (!string.IsNullOrWhiteSpace(issueParams.SearchValue))
        {
            string searchValue = issueParams.SearchValue.ToLower();
            issueQuery.Filters.Add(i =>
                i.Title.ToLower().Contains(searchValue)
                || i.StoryPoint.ToString().ToLower().Contains(searchValue)
            );
        }

        return (await uow.Issue.GetManyAsync<IssueBoardDTO>(issueQuery))
            .GroupBy(i => i.ProjectStatusId.Value)
            .Select(ig => new IssueBoardGroupDTO()
            {
                ProjectStatusId = ig.Key,
                Issues = ig.AsEnumerable(),
            });
    }

    public async Task<bool> UpdateBoardIssue(
        int projectId,
        int issueId,
        UpdateBoardIssueDTO updateBoardIssueDTO
    )
    {
        IssueEntity issueDb = await uow.Issue.GetOneAsync<IssueEntity>(
            new QueryModel<IssueEntity>()
            {
                Filters =
                {
                    i =>
                        i.Id == issueId
                        && i.ProjectId == projectId
                        && i.DeletedAt == null
                        && i.BacklogIndex == null,
                },
            }
        );

        if (issueDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "board_issue_not_found");

        mapper.Map(updateBoardIssueDTO, issueDb);
        uow.Issue.Update(issueDb);
        return await uow.Save();
    }

    public async Task<bool> MoveIssueToBacklog(
        ReqUser reqUser,
        int projectId,
        MoveIssueDTO moveIssueDTO
    )
    {
        await uow.Issue.MoveIssueToBacklog(moveIssueDTO.Issues, projectId);
        return await uow.Save();
    }
    #endregion

    #region Backlog
    public async Task<List<IssueBacklogDTO>> GetIssuesInBacklog(
        int projectId,
        IssueBacklogParams issueParams
    )
    {
        QueryModel<IssueEntity> issueQuery = new QueryModel<IssueEntity>()
        {
            OrderBy = c => c.OrderBy(i => i.BacklogIndex),
            Filters =
            {
                i =>
                    i.ProjectId == projectId
                    && i.DeletedAt == null
                    && i.BacklogIndex != null
                    && i.SprintId == null,
            },
            Includes = { i => i.Assignee.UserProfile },
        };

        if (!String.IsNullOrEmpty(issueParams.Assignees))
        {
            IEnumerable<int> assignees = issueParams
                .Assignees.Split(",")
                .Select(c => Convert.ToInt32(c));
            issueQuery.Filters.Add(i => assignees.Contains(i.AssigneeId.Value));
        }

        if (!string.IsNullOrWhiteSpace(issueParams.SearchValue))
        {
            string searchValue = issueParams.SearchValue.ToLower();
            issueQuery.Filters.Add(i =>
                i.Title.ToLower().Contains(searchValue)
                || i.StoryPoint.ToString().ToLower().Contains(searchValue)
            );
        }

        return await uow.Issue.GetManyAsync<IssueBacklogDTO>(issueQuery);
    }

    public async Task<bool> UpdateBacklogIssue(
        int projectId,
        int issueId,
        UpdateBacklogIssueDTO updateBacklogIssueDTO
    )
    {
        IssueEntity issueDb = await uow.Issue.GetOneAsync<IssueEntity>(
            new QueryModel<IssueEntity>()
            {
                Filters =
                {
                    i =>
                        i.Id == issueId
                        && i.ProjectId == projectId
                        && i.DeletedAt == null
                        && i.BacklogIndex != null
                        && i.SprintId == null,
                },
            }
        );

        if (issueDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "backlog_issue_not_found");

        mapper.Map(updateBacklogIssueDTO, issueDb);
        uow.Issue.Update(issueDb);
        return await uow.Save();
    }

    public async Task<bool> MoveIssueToSprint(
        ReqUser reqUser,
        int projectId,
        MoveIssueDTO moveIssueDTO
    )
    {
        SprintEntity availableSprint = await uow.Sprint.GetAvailableSprint(projectId);

        if (availableSprint == null)
            throw new BaseException(HttpCode.NOT_FOUND, "no_available_sprint");

        await uow.Issue.MoveIssueToSprint(moveIssueDTO.Issues, availableSprint.Id);
        return await uow.Save();
    }

    #endregion

    #region Detail
    public async Task<IssueDetailDTO> GetIssueById(int projectId, int issueId)
    {
        return await uow.Issue.GetOneAsync<IssueDetailDTO>(
            new QueryModel<IssueEntity>()
            {
                Filters =
                {
                    i => i.Id == issueId && i.ProjectId == projectId && i.DeletedAt == null,
                },
                Includes =
                {
                    i => i.Assignee.UserProfile,
                    i => i.Reporter.UserProfile,
                    i => i.ProjectStatus,
                },
            }
        );
    }

    public async Task<bool> CreateIssue(
        ReqUser reqUser,
        int projectId,
        CreateIssueDTO createIssueDTO
    )
    {
        IssueEntity issueEntity = mapper.Map<IssueEntity>(createIssueDTO);

        issueEntity.ReporterId = reqUser.Id;
        issueEntity.ProjectId = projectId;
        issueEntity.ProjectStatusId = await uow.ProjectStatus.GetStatusIdForNewIssue(projectId);
        issueEntity.ProjectStatusIndex = Math.Floor(
            uow.Issue.GetStatusIndexForNewIssue(projectId, issueEntity.ProjectStatusId.Value)
        );
        issueEntity.BacklogIndex = Math.Floor(uow.Issue.GetBacklogIndexForNewIssue(projectId));

        uow.Issue.Add(issueEntity);
        return await uow.Save();
    }

    public async Task<bool> UpdateIssue(
        ReqUser reqUser,
        int projectId,
        int issueId,
        UpdateIssueDTO updateIssueDTO
    )
    {
        IssueEntity issueDb = await uow.Issue.GetForUpdateAndDelete(projectId, reqUser.Id, issueId);

        if (issueDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "issue_not_found");

        mapper.Map(updateIssueDTO, issueDb);
        uow.Issue.Update(issueDb);
        return await uow.Save();
    }

    public async Task<bool> DeleteIssue(ReqUser reqUser, int projectId, int issueId)
    {
        IssueEntity issueDb = await uow.Issue.GetForUpdateAndDelete(projectId, reqUser.Id, issueId);

        if (issueDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "issue_not_found");

        uow.Issue.SoftDelete(issueDb);
        return await uow.Save();
    }
    #endregion
}
