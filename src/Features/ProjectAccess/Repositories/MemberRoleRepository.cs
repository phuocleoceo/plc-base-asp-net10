using AutoMapper;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.ProjectAccess.Entities;

namespace PlcBase.Features.ProjectAccess.Repositories;

public class MemberRoleRepository(DataContext db, IMapper mapper)
    : BaseRepository<MemberRoleEntity>(db, mapper),
        IMemberRoleRepository { }
