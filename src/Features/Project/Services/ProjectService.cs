using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Project.DTOs;
using PlcBase.Features.Project.Entities;
using PlcBase.Features.ProjectAccess.Services;
using PlcBase.Features.ProjectMember.Entities;
using PlcBase.Features.ProjectMember.Services;
using PlcBase.Features.ProjectStatus.Entities;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Enums;
using PlcBase.Shared.Helpers;

namespace PlcBase.Features.Project.Services;

public class ProjectService(
    IUnitOfWork uow,
    IMapper mapper,
    IPermissionHelper permissionHelper,
    IProjectPermissionService projectPermissionService
) : IProjectService
{
    public async Task<PagedList<ProjectDTO>> GetProjectsForUser(
        ReqUser reqUser,
        ProjectParams projectParams
    )
    {
        List<int> projectIds = await uow.ProjectMember.GetProjectIdsForUser(reqUser.Id);

        QueryModel<ProjectEntity> projectQuery = new QueryModel<ProjectEntity>()
        {
            OrderBy = c => c.OrderByDescending(p => p.CreatedAt),
            Filters = { p => projectIds.Contains(p.Id) && p.DeletedAt == null },
            Includes = { p => p.Leader.UserProfile },
            PageSize = projectParams.PageSize,
            PageNumber = projectParams.PageNumber,
        };

        if (!string.IsNullOrWhiteSpace(projectParams.SearchValue))
        {
            string searchValue = projectParams.SearchValue.ToLower();
            projectQuery.Filters.Add(p =>
                p.Name.ToLower().Contains(searchValue) || p.Key.ToLower().Contains(searchValue)
            );
        }

        return await uow.Project.GetPagedAsync<ProjectDTO>(projectQuery);
    }

    public async Task<ProjectDTO> GetProjectById(ReqUser reqUser, int projectId)
    {
        ProjectMemberEntity projectMember =
            await uow.ProjectMember.GetOneAsync<ProjectMemberEntity>(
                new QueryModel<ProjectMemberEntity>()
                {
                    Filters =
                    {
                        m =>
                            m.UserId == reqUser.Id
                            && m.ProjectId == projectId
                            && m.DeletedAt == null,
                    },
                }
            );

        if (projectMember == null)
            throw new BaseException(HttpCode.BAD_REQUEST, "unreachable_project");

        return await uow.Project.GetOneAsync<ProjectDTO>(
            new QueryModel<ProjectEntity>()
            {
                Filters = { m => m.Id == projectId && m.DeletedAt == null },
                Includes = { p => p.Leader.UserProfile },
            }
        );
    }

    public async Task<bool> CreateProject(ReqUser reqUser, CreateProjectDTO createProjectDTO)
    {
        try
        {
            await uow.CreateTransaction();

            double freeProject = await uow.ConfigSetting.GetValueByKey(
                ConfigSettingKey.FREE_PROJECT
            );

            int projectCount = await uow.Project.CountByCreatorId(reqUser.Id);

            if (projectCount >= freeProject)
            {
                // Payment
                double projectPrice = await uow.ConfigSetting.GetValueByKey(
                    ConfigSettingKey.PROJECT_PRICE
                );

                if (!await uow.UserProfile.MakePayment(reqUser.Id, projectPrice))
                    throw new BaseException(HttpCode.BAD_REQUEST, "not_enough_credit");
                await uow.Save();
            }

            ProjectEntity projectEntity = mapper.Map<ProjectEntity>(createProjectDTO);
            projectEntity.CreatorId = reqUser.Id;
            projectEntity.LeaderId = reqUser.Id;

            uow.Project.Add(projectEntity);
            await uow.Save();

            // Project member
            uow.ProjectMember.Add(
                new ProjectMemberEntity() { UserId = reqUser.Id, ProjectId = projectEntity.Id }
            );

            // Project Status
            uow.ProjectStatus.Add(
                new ProjectStatusEntity()
                {
                    Name = "To Do",
                    Index = 0,
                    ProjectId = projectEntity.Id,
                }
            );

            await uow.Save();

            await uow.CommitTransaction();
            return true;
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }

    public async Task<bool> UpdateProject(
        ReqUser reqUser,
        int projectId,
        UpdateProjectDTO updateProjectDTO
    )
    {
        ProjectEntity projectDb = await uow.Project.GetByIdAndOwner(reqUser.Id, projectId);

        if (projectDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "project_not_found");

        mapper.Map(updateProjectDTO, projectDb);
        uow.Project.Update(projectDb);
        return await uow.Save();
    }

    public async Task<bool> DeleteProject(ReqUser reqUser, int projectId)
    {
        try
        {
            await uow.CreateTransaction();

            ProjectEntity projectDb = await uow.Project.GetByIdAndOwner(reqUser.Id, projectId);

            if (projectDb == null)
                throw new BaseException(HttpCode.NOT_FOUND, "project_not_found");

            uow.Project.SoftDelete(projectDb);
            await uow.Save();

            await uow.ProjectMember.SoftDeleteMemberForProject(projectId);
            await uow.Save();

            await uow.CommitTransaction();
            return true;
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetPermissionsInProjectForUser(
        ReqUser reqUser,
        int projectId
    )
    {
        ProjectEntity projectEntity = await uow.Project.FindByIdAsync(projectId);

        if (projectEntity == null)
        {
            throw new BaseException(HttpCode.NOT_FOUND, "project_not_found");
        }

        if (projectEntity.LeaderId == reqUser.Id)
        {
            return permissionHelper.GetAllPermissions().Select(p => p.Key);
        }

        return await projectPermissionService.GetPermissionKeysOfRole(
            await uow.ProjectMember.GetRoleInProjectForUser(reqUser.Id, projectId)
        );
    }
}
