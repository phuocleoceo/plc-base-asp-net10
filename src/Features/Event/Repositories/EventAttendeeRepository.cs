using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.Event.Entities;

namespace PlcBase.Features.Event.Repositories;

public class EventAttendeeRepository(DataContext db, IMapper mapper)
    : BaseRepository<EventAttendeeEntity>(db, mapper),
        IEventAttendeeRepository
{
    public async Task<HashSet<int>> GetAttendeeIdsForEvent(int eventId)
    {
        return
        [
            .. (
                await _dbSet
                    .Where(ea => ea.EventId == eventId)
                    .Select(ea => ea.UserId)
                    .ToListAsync()
            ),
        ];
    }

    public async Task RemoveAttendeesByUserIds(IEnumerable<int> userIds)
    {
        IEnumerable<EventAttendeeEntity> attendeeEntities = await _dbSet
            .Where(ea => userIds.Contains(ea.UserId))
            .ToListAsync();

        RemoveRange(attendeeEntities);
    }
}
