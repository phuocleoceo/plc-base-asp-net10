using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Features.Project.Entities;

namespace PlcBase.Features.Project.Repositories;

public interface IProjectRepository : IBaseRepository<ProjectEntity>
{
    Task<ProjectEntity> GetByIdAndOwner(int creatorId, int projectId);

    Task<int> CountByCreatorId(int creatorId);
}
