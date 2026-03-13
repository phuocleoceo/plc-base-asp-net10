using PlcBase.Base.Repository;
using PlcBase.Features.ProjectAccess.Entities;

namespace PlcBase.Features.ProjectAccess.Repositories;

public interface IProjectPermissionRepository : IBaseRepository<ProjectPermissionEntity>
{
    Task<List<ProjectPermissionEntity>> GetForProjectRole(int projectRoleId);

    Task<List<ProjectPermissionEntity>> GetForProjectRoles(List<int> projectRoleIds);
}
