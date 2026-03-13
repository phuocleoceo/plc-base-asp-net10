using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Issue.Entities;
using PlcBase.Features.Sprint.DTOs;
using PlcBase.Features.Sprint.Entities;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Sprint.Services;

public class SprintService(IUnitOfWork uow, IMapper mapper) : ISprintService
{
    public async Task<SprintDTO> GetAvailableSprint(int projectId)
    {
        return await uow.Sprint.GetOneAsync<SprintDTO>(
            new QueryModel<SprintEntity>()
            {
                Filters = { i => i.ProjectId == projectId && i.CompletedAt == null },
            }
        );
    }

    public async Task<SprintDTO> GetSprintById(int projectId, int sprintId)
    {
        return await uow.Sprint.GetOneAsync<SprintDTO>(
            new QueryModel<SprintEntity>()
            {
                Filters = { i => i.Id == sprintId && i.ProjectId == projectId },
            }
        );
    }

    public async Task<bool> CreateSprint(
        ReqUser reqUser,
        int projectId,
        CreateSprintDTO createSprintDTO
    )
    {
        SprintEntity sprintEntity = mapper.Map<SprintEntity>(createSprintDTO);
        sprintEntity.ProjectId = projectId;

        uow.Sprint.Add(sprintEntity);
        return await uow.Save();
    }

    public async Task<bool> UpdateSprint(
        ReqUser reqUser,
        int projectId,
        int sprintId,
        UpdateSprintDTO updateSprintDTO
    )
    {
        SprintEntity sprintDb =
            await uow.Sprint.GetForUpdateAndDelete(projectId, sprintId)
            ?? throw new BaseException(HttpCode.NOT_FOUND, "sprint_not_found");
        mapper.Map(updateSprintDTO, sprintDb);
        uow.Sprint.Update(sprintDb);
        return await uow.Save();
    }

    public async Task<bool> DeleteSprint(ReqUser reqUser, int projectId, int sprintId)
    {
        try
        {
            await uow.CreateTransaction();

            SprintEntity sprintDb =
                await uow.Sprint.GetForUpdateAndDelete(projectId, sprintId)
                ?? throw new BaseException(HttpCode.NOT_FOUND, "sprint_not_found");
            uow.Sprint.Remove(sprintDb);

            await uow.Issue.MoveIssueFromSprintToBacklog(sprintId, projectId);

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

    public async Task<bool> StartSprint(ReqUser reqUser, int projectId, int sprintId)
    {
        SprintEntity sprintDb =
            await uow.Sprint.GetForUpdateAndDelete(projectId, sprintId)
            ?? throw new BaseException(HttpCode.NOT_FOUND, "sprint_not_found");
        sprintDb.StartedAt = TimeUtility.Now();
        uow.Sprint.Update(sprintDb);
        return await uow.Save();
    }

    public async Task<bool> CompleteSprint(
        ReqUser reqUser,
        int projectId,
        int sprintId,
        CompleteSprintDTO completeSprintDTO
    )
    {
        try
        {
            await uow.CreateTransaction();

            if (
                completeSprintDTO.MoveType != "backlog"
                && completeSprintDTO.MoveType != "next_sprint"
            )
                throw new BaseException(HttpCode.BAD_REQUEST, "invalid_move_type");

            SprintEntity sprintDb =
                await uow.Sprint.GetForUpdateAndDelete(projectId, sprintId)
                ?? throw new BaseException(HttpCode.NOT_FOUND, "sprint_not_found");
            sprintDb.CompletedAt = TimeUtility.Now();
            uow.Sprint.Update(sprintDb);
            await uow.Save();

            List<IssueEntity> completedIssues = await uow.Issue.GetByIds(
                completeSprintDTO.CompletedIssues
            );

            completedIssues.ForEach(i =>
            {
                i.SprintId = null;
                i.BacklogIndex = null;
                uow.Issue.Update(i);
            });
            await uow.Save();

            if (completeSprintDTO.MoveType == "backlog")
            {
                await uow.Issue.MoveIssueToBacklog(completeSprintDTO.UnCompletedIssues, projectId);
                await uow.Save();
            }

            if (completeSprintDTO.MoveType == "next_sprint")
            {
                SprintEntity nextSprint = new()
                {
                    Title = "next_sprint",
                    Goal = "",
                    ProjectId = projectId,
                };

                uow.Sprint.Add(nextSprint);
                await uow.Save();

                await uow.Issue.MoveIssueToSprint(
                    completeSprintDTO.UnCompletedIssues,
                    nextSprint.Id
                );
                await uow.Save();
            }

            await uow.CommitTransaction();
            return true;
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }
}
