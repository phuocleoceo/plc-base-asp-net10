using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.Sprint.Entities;

namespace PlcBase.Features.Sprint.Repositories;

public class SprintRepository(DataContext db, IMapper mapper)
    : BaseRepository<SprintEntity>(db, mapper),
        ISprintRepository
{
    public async Task<SprintEntity> GetForUpdateAndDelete(int projectId, int sprintId)
    {
        return await GetOneAsync<SprintEntity>(
            new QueryModel<SprintEntity>()
            {
                Filters = { i => i.Id == sprintId && i.ProjectId == projectId },
            }
        );
    }

    public async Task<SprintEntity> GetAvailableSprint(int projectId)
    {
        return await GetOneAsync<SprintEntity>(
            new QueryModel<SprintEntity>()
            {
                Filters = { i => i.ProjectId == projectId && i.CompletedAt == null },
            }
        );
    }
}
