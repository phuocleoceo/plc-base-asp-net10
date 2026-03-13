using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Project.Entities;
using PlcBase.Features.ProjectMember.DTOs;
using PlcBase.Features.ProjectMember.Entities;
using PlcBase.Shared.Constants;

namespace PlcBase.Features.ProjectMember.Services;

public class ProjectMemberService(IUnitOfWork uow) : IProjectMemberService
{
    public async Task<PagedList<ProjectMemberDTO>> GetMembersForProject(
        int projectId,
        ProjectMemberParams projectMemberParams
    )
    {
        QueryModel<ProjectMemberEntity> memberQuery = new()
        {
            OrderBy = c => c.OrderByDescending(pm => pm.CreatedAt),
            Filters = { pm => pm.ProjectId == projectId },
            Includes = { pm => pm.User.UserProfile },
            PageSize = projectMemberParams.PageSize,
            PageNumber = projectMemberParams.PageNumber,
        };

        if (!projectMemberParams.WithDeleted)
        {
            memberQuery.Filters.Add(i => i.DeletedAt == null);
        }

        if (!string.IsNullOrWhiteSpace(projectMemberParams.SearchValue))
        {
            string searchValue = projectMemberParams.SearchValue.ToLower();
            memberQuery.Filters.Add(i =>
                i.User.Email.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase)
                || i.User.UserProfile.DisplayName.Contains(
                    searchValue,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }

        return await uow.ProjectMember.GetPagedAsync<ProjectMemberDTO>(memberQuery);
    }

    public async Task<List<ProjectMemberSelectDTO>> GetMembersForSelect(int projectId)
    {
        return await uow.ProjectMember.GetManyAsync<ProjectMemberSelectDTO>(
            new QueryModel<ProjectMemberEntity>()
            {
                Filters = { i => i.ProjectId == projectId && i.DeletedAt == null },
                Includes = { i => i.User.UserProfile },
            }
        );
    }

    public async Task<bool> DeleteProjectMember(int projectId, int projectMemberId)
    {
        ProjectMemberEntity projectMemberDb =
            await uow.ProjectMember.FindByIdAsync(projectMemberId)
            ?? throw new BaseException(HttpCode.NOT_FOUND, "project_member_not_found");

        if (projectMemberDb.ProjectId != projectId)
            throw new BaseException(HttpCode.BAD_REQUEST, "invalid_project_member");

        uow.ProjectMember.SoftDelete(projectMemberDb);
        return await uow.Save();
    }

    public async Task<bool> LeaveProject(ReqUser reqUser, int projectId)
    {
        ProjectEntity projectDb = await uow.Project.FindByIdAsync(projectId);

        if (projectDb.LeaderId == reqUser.Id)
            throw new BaseException(HttpCode.BAD_REQUEST, "leader_cannot_leave");

        ProjectMemberEntity projectMemberDb =
            await uow.ProjectMember.GetOneAsync<ProjectMemberEntity>(
                new QueryModel<ProjectMemberEntity>()
                {
                    Filters =
                    {
                        pm =>
                            pm.UserId == reqUser.Id
                            && pm.ProjectId == projectId
                            && pm.DeletedAt == null,
                    },
                }
            ) ?? throw new BaseException(HttpCode.NOT_FOUND, "project_member_not_found");

        uow.ProjectMember.SoftDelete(projectMemberDb);
        return await uow.Save();
    }
}
