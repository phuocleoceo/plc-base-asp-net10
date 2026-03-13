using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Issue.Entities;
using PlcBase.Features.ProjectStatus.DTOs;
using PlcBase.Features.ProjectStatus.Entities;
using PlcBase.Shared.Constants;

namespace PlcBase.Features.ProjectStatus.Services;

public class ProjectStatusService(IUnitOfWork uow, IMapper mapper) : IProjectStatusService
{
    public async Task<List<ProjectStatusDTO>> GetProjectStatusForProject(int projectId)
    {
        return await uow.ProjectStatus.GetManyAsync<ProjectStatusDTO>(
            new QueryModel<ProjectStatusEntity>()
            {
                OrderBy = c => c.OrderBy(s => s.Index),
                Filters = { s => s.ProjectId == projectId && s.DeletedAt == null },
            }
        );
    }

    public async Task<bool> CreateProjectStatus(
        int projectId,
        CreateProjectStatusDTO createProjectStatusDTO
    )
    {
        ProjectStatusEntity projectStatusEntity = mapper.Map<ProjectStatusEntity>(
            createProjectStatusDTO
        );
        projectStatusEntity.Index = await uow.ProjectStatus.GetIndexForNewStatus(projectId);
        projectStatusEntity.ProjectId = projectId;

        uow.ProjectStatus.Add(projectStatusEntity);
        return await uow.Save();
    }

    public async Task<bool> UpdateProjectStatus(
        int projectId,
        int projectStatusId,
        UpdateProjectStatusDTO updateProjectStatusDTO
    )
    {
        ProjectStatusEntity projectStatusDb =
            await uow.ProjectStatus.GetOneAsync<ProjectStatusEntity>(
                new QueryModel<ProjectStatusEntity>()
                {
                    Filters = { s => s.Id == projectStatusId && s.ProjectId == projectId },
                }
            ) ?? throw new BaseException(HttpCode.NOT_FOUND, "project_status_not_found");

        mapper.Map(updateProjectStatusDTO, projectStatusDb);
        uow.ProjectStatus.Update(projectStatusDb);
        return await uow.Save();
    }

    public async Task<bool> DeleteProjectStatus(int projectId, int projectStatusId)
    {
        try
        {
            await uow.CreateTransaction();

            int countStatus = await uow.ProjectStatus.CountAsync(ps => ps.ProjectId == projectId);
            if (countStatus <= 1)
                throw new BaseException(HttpCode.BAD_REQUEST, "must_have_at_least_one_status");

            ProjectStatusEntity currentStatus =
                await uow.ProjectStatus.FindByIdAsync(projectStatusId)
                ?? throw new BaseException(HttpCode.NOT_FOUND, "project_status_not_found");

            // Update new status for issues
            int? newStatusId = await uow.ProjectStatus.GetNewStatusIdForIssueWhenDeletingStatus(
                projectId,
                currentStatus.Id
            );

            List<IssueEntity> issues = await uow.Issue.GetManyAsync<IssueEntity>(
                new QueryModel<IssueEntity>()
                {
                    Filters =
                    {
                        i =>
                            i.ProjectId == projectId
                            && i.ProjectStatusId == projectStatusId
                            && i.DeletedAt == null,
                    },
                }
            );

            double statusIndex = uow.Issue.GetStatusIndexForNewIssue(projectId, newStatusId.Value);
            foreach (IssueEntity issue in issues)
            {
                issue.ProjectStatusId = newStatusId;
                issue.ProjectStatusIndex = statusIndex++;
                uow.Issue.Update(issue);
            }
            await uow.Save();

            // Remove status
            uow.ProjectStatus.Remove(currentStatus);
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
}
