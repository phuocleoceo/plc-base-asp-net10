using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.Event.Entities;

namespace PlcBase.Features.Event.Repositories;

public class EventRepository(DataContext db, IMapper mapper)
    : BaseRepository<EventEntity>(db, mapper),
        IEventRepository
{
    public async Task<EventEntity> GetForUpdateAndDelete(int creatorId, int projectId, int eventId)
    {
        return await GetOneAsync<EventEntity>(
            new QueryModel<EventEntity>()
            {
                Filters =
                {
                    e => e.Id == eventId && e.ProjectId == projectId && e.CreatorId == creatorId,
                },
            }
        );
    }
}
