using PlcBase.Common.Repositories;
using PlcBase.Features.AccessControl.DTOs;

namespace PlcBase.Features.AccessControl.Services;

public class AccessControlService(IUnitOfWork uow) : IAccessControlService
{
    public async Task<List<RoleDTO>> GetRoles()
    {
        return await uow.Role.GetManyAsync<RoleDTO>();
    }
}
