using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.ProjectStatus.Entities;

namespace PlcBase.Features.ProjectStatus.Repositories;

public class ProjectStatusRepository(DataContext db, IMapper mapper)
    : BaseRepository<ProjectStatusEntity>(db, mapper),
        IProjectStatusRepository
{
    public async Task<double> GetIndexForNewStatus(int projectId)
    {
        ProjectStatusEntity projectStatus = await GetOneAsync<ProjectStatusEntity>(
            new QueryModel<ProjectStatusEntity>()
            {
                OrderBy = c => c.OrderByDescending(s => s.Index),
                Filters = { s => s.ProjectId == projectId },
            }
        );

        return projectStatus?.Index + 1 ?? 0;
    }

    public async Task<int?> GetStatusIdForNewIssue(int projectId)
    {
        ProjectStatusEntity projectStatus = await GetOneAsync<ProjectStatusEntity>(
            new QueryModel<ProjectStatusEntity>()
            {
                OrderBy = c => c.OrderBy(s => s.Index),
                Filters = { s => s.ProjectId == projectId },
            }
        );

        return projectStatus?.Id;
    }

    public async Task<int?> GetNewStatusIdForIssueWhenDeletingStatus(
        int projectId,
        int deletingStatusId
    )
    {
        ProjectStatusEntity projectStatus = await GetOneAsync<ProjectStatusEntity>(
            new QueryModel<ProjectStatusEntity>()
            {
                OrderBy = c => c.OrderBy(s => s.Index),
                Filters = { s => s.ProjectId == projectId && s.Id != deletingStatusId },
            }
        );

        return projectStatus?.Id;
    }
}
