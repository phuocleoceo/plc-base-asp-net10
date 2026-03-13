using PlcBase.Base.Repository;
using PlcBase.Features.ProjectStatus.Entities;

namespace PlcBase.Features.ProjectStatus.Repositories;

public interface IProjectStatusRepository : IBaseRepository<ProjectStatusEntity>
{
    Task<double> GetIndexForNewStatus(int projectId);

    Task<int?> GetStatusIdForNewIssue(int projectId);

    Task<int?> GetNewStatusIdForIssueWhenDeletingStatus(int projectId, int deletingStatusId);
}
