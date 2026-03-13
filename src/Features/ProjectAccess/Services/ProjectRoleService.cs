using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.ProjectAccess.DTOs;
using PlcBase.Features.ProjectAccess.Entities;
using PlcBase.Shared.Constants;

namespace PlcBase.Features.ProjectAccess.Services;

public class ProjectRoleService(IUnitOfWork uow, IMapper mapper) : IProjectRoleService
{
    public async Task<List<ProjectRoleDTO>> GetAllProjectRoles()
    {
        return await uow.ProjectRole.GetManyAsync<ProjectRoleDTO>(
            new QueryModel<ProjectRoleEntity>() { OrderBy = c => c.OrderBy(r => r.CreatedAt) }
        );
    }

    public async Task<PagedList<ProjectRoleDTO>> GetProjectRoles(ProjectRoleParams roleParams)
    {
        QueryModel<ProjectRoleEntity> roleQuery = new()
        {
            OrderBy = c => c.OrderBy(r => r.CreatedAt),
            PageSize = roleParams.PageSize,
            PageNumber = roleParams.PageNumber,
        };

        if (!string.IsNullOrWhiteSpace(roleParams.SearchValue))
        {
            string searchValue = roleParams.SearchValue.ToLower();
            roleQuery.Filters.Add(r =>
                r.Name.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase)
                || r.Description.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        return await uow.ProjectRole.GetPagedAsync<ProjectRoleDTO>(roleQuery);
    }

    public async Task<ProjectRoleDTO> GetProjectRoleById(int projectRoleId)
    {
        return await uow.ProjectRole.GetOneAsync<ProjectRoleDTO>(
                new QueryModel<ProjectRoleEntity>() { Filters = { r => r.Id == projectRoleId } }
            ) ?? throw new BaseException(HttpCode.NOT_FOUND, "project_role_not_found");
    }

    public async Task<bool> CreateProjectRole(CreateProjectRoleDTO createRoleDTO)
    {
        ProjectRoleEntity projectRoleEntity = mapper.Map<ProjectRoleEntity>(createRoleDTO);

        uow.ProjectRole.Add(projectRoleEntity);
        return await uow.Save();
    }

    public async Task<bool> UpdateProjectRole(int projectRoleId, UpdateProjectRoleDTO updateRoleDTO)
    {
        ProjectRoleEntity projectRoleDb =
            await uow.ProjectRole.FindByIdAsync(projectRoleId)
            ?? throw new BaseException(HttpCode.NOT_FOUND, "project_role_not_found");

        mapper.Map(updateRoleDTO, projectRoleDb);
        uow.ProjectRole.Update(projectRoleDb);
        return await uow.Save();
    }

    public async Task<bool> DeleteProjectRole(int projectRoleId)
    {
        ProjectRoleEntity projectRoleDb =
            await uow.ProjectRole.FindByIdAsync(projectRoleId)
            ?? throw new BaseException(HttpCode.NOT_FOUND, "project_role_not_found");

        uow.ProjectRole.Remove(projectRoleDb);
        return await uow.Save();
    }
}
