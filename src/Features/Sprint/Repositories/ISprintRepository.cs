using PlcBase.Base.Repository;
using PlcBase.Features.Sprint.Entities;

namespace PlcBase.Features.Sprint.Repositories;

public interface ISprintRepository : IBaseRepository<SprintEntity>
{
    Task<SprintEntity> GetForUpdateAndDelete(int projectId, int sprintId);

    Task<SprintEntity> GetAvailableSprint(int projectId);
}
