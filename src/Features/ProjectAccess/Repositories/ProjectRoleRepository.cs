using AutoMapper;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.ProjectAccess.Entities;

namespace PlcBase.Features.ProjectAccess.Repositories;

public class ProjectRoleRepository(DataContext db, IMapper mapper)
    : BaseRepository<ProjectRoleEntity>(db, mapper),
        IProjectRoleRepository { }
