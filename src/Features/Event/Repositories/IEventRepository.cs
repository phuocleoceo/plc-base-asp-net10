using PlcBase.Base.Repository;
using PlcBase.Features.Event.Entities;

namespace PlcBase.Features.Event.Repositories;

public interface IEventRepository : IBaseRepository<EventEntity>
{
    Task<EventEntity> GetForUpdateAndDelete(int creatorId, int projectId, int eventId);
}
