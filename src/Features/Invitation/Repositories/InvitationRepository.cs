using AutoMapper;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.Invitation.Entities;

namespace PlcBase.Features.Invitation.Repositories;

public class InvitationRepository(DataContext db, IMapper mapper)
    : BaseRepository<InvitationEntity>(db, mapper),
        IInvitationRepository { }
