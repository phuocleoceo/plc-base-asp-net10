using AutoMapper;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.AccessControl.Entities;

namespace PlcBase.Features.AccessControl.Repositories;

public class RoleRepository(DataContext db, IMapper mapper)
    : BaseRepository<RoleEntity>(db, mapper),
        IRoleRepository { }
