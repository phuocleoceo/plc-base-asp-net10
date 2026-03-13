using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.ProjectMember.Entities;

namespace PlcBase.Features.ProjectMember.Repositories;

public class ProjectMemberRepository(DataContext db, IMapper mapper)
    : BaseRepository<ProjectMemberEntity>(db, mapper),
        IProjectMemberRepository
{
    public async Task<List<int>> GetProjectIdsForUser(int userId)
    {
        return await _dbSet
            .Where(m => m.UserId == userId && m.DeletedAt == null)
            .Select(m => m.ProjectId)
            .ToListAsync();
    }

    public async Task SoftDeleteMemberForProject(int projectId)
    {
        await _dbSet
            .Where(m => m.ProjectId == projectId && m.DeletedAt == null)
            .ForEachAsync(m => SoftDelete(m));
    }

    public async Task<List<int>> GetRoleInProjectForUser(int userId, int projectId)
    {
        return await _dbSet
            .Where(pm => pm.UserId == userId && pm.ProjectId == projectId)
            .Include(pm => pm.MemberRoles)
                .ThenInclude(mr => mr.ProjectRole)
            .SelectMany(pm => pm.MemberRoles)
            .Select(mr => mr.ProjectRoleId)
            .ToListAsync();
    }
}
