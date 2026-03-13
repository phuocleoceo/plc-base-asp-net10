using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.Event.DTOs;
using PlcBase.Features.Event.Services;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Event.Controllers;

public class EventController(IEventService eventService) : BaseController
{
    [HttpGet("/api/project/{projectId}/event")]
    [Authorize]
    public async Task<SuccessResponse<List<EventDTO>>> GetEvents(
        int projectId,
        [FromQuery] EventParams eventParams
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();
        return HttpContext.Success(await eventService.GetEvents(reqUser, projectId, eventParams));
    }

    [HttpGet("/api/project/{projectId}/event/{eventId}")]
    [Authorize]
    public async Task<SuccessResponse<EventDetailDTO>> GetEventDetail(int projectId, int eventId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();
        return HttpContext.Success(await eventService.GetEventDetail(reqUser, projectId, eventId));
    }

    [HttpPost("/api/project/{projectId}/event")]
    [Authorize]
    public async Task<SuccessResponse<bool>> CreateEvent(
        int projectId,
        [FromBody] CreateEventDTO createEventDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await eventService.CreateEvent(reqUser, projectId, createEventDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("/api/project/{projectId}/event/{eventId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> UpdateEvent(
        int projectId,
        int eventId,
        [FromBody] UpdateEventDTO updateEventDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await eventService.UpdateEvent(reqUser, projectId, eventId, updateEventDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpDelete("/api/project/{projectId}/event/{eventId}")]
    [Authorize]
    public async Task<SuccessResponse<bool>> DeleteEvent(int projectId, int eventId)
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await eventService.DeleteEvent(reqUser, projectId, eventId))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
