using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Event.DTOs;
using PlcBase.Features.Event.Entities;
using PlcBase.Shared.Constants;

namespace PlcBase.Features.Event.Services;

public class EventService(IUnitOfWork uow, IMapper mapper) : IEventService
{
    public async Task<List<EventDTO>> GetEvents(
        ReqUser reqUser,
        int projectId,
        EventParams eventParams
    )
    {
        QueryModel<EventEntity> eventQuery = new()
        {
            Includes = { e => e.Attendees },
            Filters =
            {
                e => e.ProjectId == projectId,
                e => e.Attendees.Select(a => a.UserId).Contains(reqUser.Id),
                e =>
                    (eventParams.Start <= e.StartTime && e.StartTime < eventParams.End)
                    || (eventParams.Start <= e.EndTime && e.EndTime < eventParams.End),
            },
        };

        return await uow.Event.GetManyAsync<EventDTO>(eventQuery);
    }

    public async Task<EventDetailDTO> GetEventDetail(ReqUser reqUser, int projectId, int eventId)
    {
        return await uow.Event.GetOneAsync<EventDetailDTO>(
            new QueryModel<EventEntity>()
            {
                Filters = { e => e.Id == eventId && e.ProjectId == projectId },
            }
        );
    }

    public async Task<bool> CreateEvent(
        ReqUser reqUser,
        int projectId,
        CreateEventDTO createEventDTO
    )
    {
        try
        {
            await uow.CreateTransaction();

            EventEntity eventEntity = mapper.Map<EventEntity>(createEventDTO);

            eventEntity.CreatorId = reqUser.Id;
            eventEntity.ProjectId = projectId;
            uow.Event.Add(eventEntity);
            await uow.Save();

            IEnumerable<EventAttendeeEntity> eventAttendees = createEventDTO.AttendeeIds.Select(
                ea => new EventAttendeeEntity() { UserId = ea, EventId = eventEntity.Id }
            );

            // Mặc định creator luôn luôn là người có tham dự
            if (!(createEventDTO.AttendeeIds.Contains(reqUser.Id)))
                eventAttendees = eventAttendees.Append(
                    new EventAttendeeEntity() { UserId = reqUser.Id, EventId = eventEntity.Id }
                );

            uow.EventAttendee.AddRange(eventAttendees);
            await uow.Save();

            await uow.CommitTransaction();
            return true;
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }

    public async Task<bool> UpdateEvent(
        ReqUser reqUser,
        int projectId,
        int eventId,
        UpdateEventDTO updateEventDTO
    )
    {
        try
        {
            await uow.CreateTransaction();

            EventEntity eventDb =
                await uow.Event.GetForUpdateAndDelete(reqUser.Id, projectId, eventId)
                ?? throw new BaseException(HttpCode.NOT_FOUND, "event_not_found");
            mapper.Map(updateEventDTO, eventDb);
            uow.Event.Update(eventDb);

            HashSet<int> currentAttendees = await uow.EventAttendee.GetAttendeeIdsForEvent(eventId);

            // Những userId Db không có, update data có => thêm mới
            IEnumerable<int> createAttendees = updateEventDTO.AttendeeIds.Except(currentAttendees);
            // Những userId DB có, update data không có => gỡ đi , không gỡ đi creator của event
            IEnumerable<int> removeAttendees = currentAttendees
                .Except(updateEventDTO.AttendeeIds)
                .Where(a => a != eventDb.CreatorId);
            // Những userId cả Db và update data có thì giữ nguyên

            uow.EventAttendee.AddRange(
                createAttendees.Select(attendeeId => new EventAttendeeEntity()
                {
                    UserId = attendeeId,
                    EventId = eventId,
                })
            );

            await uow.EventAttendee.RemoveAttendeesByUserIds(removeAttendees);

            await uow.Save();
            await uow.CommitTransaction();
            return true;
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }

    public async Task<bool> DeleteEvent(ReqUser reqUser, int projectId, int eventId)
    {
        EventEntity eventDb =
            await uow.Event.GetForUpdateAndDelete(reqUser.Id, projectId, eventId)
            ?? throw new BaseException(HttpCode.NOT_FOUND, "event_not_found");
        uow.Event.Remove(eventDb);
        return await uow.Save();
    }
}
