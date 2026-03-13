using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.ProjectAccess.Entities;

namespace PlcBase.Features.ProjectAccess.Repositories;

public class ProjectPermissionRepository(DataContext db, IMapper mapper)
    : BaseRepository<ProjectPermissionEntity>(db, mapper),
        IProjectPermissionRepository
{
    public async Task<List<ProjectPermissionEntity>> GetForProjectRole(int projectRoleId)
    {
        return await GetManyAsync<ProjectPermissionEntity>(
            new QueryModel<ProjectPermissionEntity>()
            {
                Filters = { pm => pm.ProjectRoleId == projectRoleId },
            }
        );
    }

    public async Task<List<ProjectPermissionEntity>> GetForProjectRoles(List<int> projectRoleIds)
    {
        return await GetManyAsync<ProjectPermissionEntity>(
            new QueryModel<ProjectPermissionEntity>()
            {
                Filters = { pm => projectRoleIds.Contains(pm.ProjectRoleId) },
            }
        );
    }
}
