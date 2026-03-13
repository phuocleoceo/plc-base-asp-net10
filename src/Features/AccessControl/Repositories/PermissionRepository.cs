using AutoMapper;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.AccessControl.Entities;

namespace PlcBase.Features.AccessControl.Repositories;

public class PermissionRepository(DataContext db, IMapper mapper)
    : BaseRepository<PermissionEntity>(db, mapper),
        IPermisisonRepository { }
