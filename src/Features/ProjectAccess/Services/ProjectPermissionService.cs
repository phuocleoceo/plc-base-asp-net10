using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.ProjectAccess.DTOs;
using PlcBase.Features.ProjectAccess.Entities;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Helpers;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.ProjectAccess.Services;

public class ProjectPermissionService(
    IPermissionHelper permissionHelper,
    IRedisHelper redisHelper,
    IUnitOfWork uow,
    IMapper mapper
) : IProjectPermissionService
{
    public async Task<IEnumerable<ProjectPermissionGroupDTO>> GetForProjectRole(int projectRoleId)
    {
        List<PermissionContent> allPermissions = permissionHelper.GetAllPermissions();

        IEnumerable<string> projectPermissions = (
            await uow.ProjectPermission.GetForProjectRole(projectRoleId)
        ).Select(pm => pm.Key);

        return allPermissions
            .GroupBy(p => p.Key.Split(".")[0])
            .Select(p => new ProjectPermissionGroupDTO()
            {
                Module = p.Key,
                Children = p.Select(m => new ProjectPermissionDTO()
                    {
                        Key = m.Key,
                        Description = m.Description,
                        IsGranted = projectPermissions.Contains(m.Key),
                    })
                    .OrderBy(m => m.Key),
            })
            .OrderBy(p => p.Module);
    }

    public async Task<bool> CreateProjectPermission(
        int projectRoleId,
        CreateProjectPermissionDTO createProjectPermissionDTO
    )
    {
        ProjectPermissionEntity projectPermissionEntity = mapper.Map<ProjectPermissionEntity>(
            createProjectPermissionDTO
        );
        projectPermissionEntity.ProjectRoleId = projectRoleId;

        uow.ProjectPermission.Add(projectPermissionEntity);
        await redisHelper.RemoveMapCache(
            GetPermissionKeysOfRoleRedisKey(),
            projectRoleId.ToString()
        );
        return await uow.Save();
    }

    public async Task<bool> DeleteProjectPermission(int projectRoleId, string projectPermissionKey)
    {
        ProjectPermissionEntity projectPermissionDb =
            await uow.ProjectPermission.GetOneAsync<ProjectPermissionEntity>(
                new QueryModel<ProjectPermissionEntity>()
                {
                    Filters =
                    {
                        pm => pm.Key == projectPermissionKey && pm.ProjectRoleId == projectRoleId,
                    },
                }
            ) ?? throw new BaseException(HttpCode.NOT_FOUND, "project_permission_not_found");

        uow.ProjectPermission.Remove(projectPermissionDb);
        await redisHelper.RemoveMapCache(
            GetPermissionKeysOfRoleRedisKey(),
            projectRoleId.ToString()
        );
        return await uow.Save();
    }

    public async Task<IEnumerable<string>> GetPermissionKeysOfRole(List<int> projectRoleIds)
    {
        if (projectRoleIds.Count == 0)
        {
            return [];
        }

        Dictionary<string, List<string>> permissionsOfRole = await redisHelper.GetMapCache<
            List<string>
        >(
            GetPermissionKeysOfRoleRedisKey(),
            [.. projectRoleIds.Select(projectRoleId => projectRoleId.ToString())]
        );

        List<string> permissionKeysInCache = [];
        List<int> projectRoleIdsNeedCachePermission = [];

        foreach (int projectRoleId in projectRoleIds)
        {
            permissionsOfRole.TryGetValue(projectRoleId.ToString(), out List<string> permissionKey);
            if (permissionKey == null)
            {
                projectRoleIdsNeedCachePermission.Add(projectRoleId);
                continue;
            }
            permissionKeysInCache.AddRange(permissionKey);
        }

        if (projectRoleIdsNeedCachePermission.Count == 0)
        {
            return permissionKeysInCache;
        }

        List<ProjectPermissionEntity> projectPermissions =
            await uow.ProjectPermission.GetForProjectRoles(projectRoleIdsNeedCachePermission);

        Dictionary<string, List<string>> permissionsNeedCache = projectPermissions
            .GroupBy(pm => pm.ProjectRoleId.ToString())
            .ToDictionary(group => group.Key, group => group.Select(pm => pm.Key).ToList());

        permissionKeysInCache.AddRange(
            projectPermissions.Select(projectPermission => projectPermission.Key)
        );

        await redisHelper.SetMapCache(GetPermissionKeysOfRoleRedisKey(), permissionsNeedCache);

        return permissionKeysInCache;
    }

    private static string GetPermissionKeysOfRoleRedisKey()
    {
        return RedisUtility.GetKey<ProjectRoleDTO>("permissions");
    }
}
