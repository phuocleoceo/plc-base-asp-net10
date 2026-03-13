using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.ProjectAccess.DTOs;
using PlcBase.Features.ProjectAccess.Entities;
using PlcBase.Shared.Constants;

namespace PlcBase.Features.ProjectAccess.Services;

public class MemberRoleService(IUnitOfWork uow, IMapper mapper) : IMemberRoleService
{
    public async Task<List<MemberRoleDTO>> GetProjectRoleForMember(int projectMemberId)
    {
        return await uow.MemberRole.GetManyAsync<MemberRoleDTO>(
            new QueryModel<MemberRoleEntity>()
            {
                Filters = { mr => mr.ProjectMemberId == projectMemberId },
            }
        );
    }

    public async Task<bool> CreateMemberRole(CreateMemberRoleDTO createMemberRoleDTO)
    {
        MemberRoleEntity memberRoleEntity = mapper.Map<MemberRoleEntity>(createMemberRoleDTO);

        uow.MemberRole.Add(memberRoleEntity);
        return await uow.Save();
    }

    public async Task<bool> DeleteMemberRole(int projectMemberId, int projectRoleId)
    {
        MemberRoleEntity memberRoleDb = await uow.MemberRole.GetOneAsync<MemberRoleEntity>(
            new QueryModel<MemberRoleEntity>()
            {
                Filters =
                {
                    mr =>
                        mr.ProjectMemberId == projectMemberId && mr.ProjectRoleId == projectRoleId,
                },
            }
        );

        if (memberRoleDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "member_role_not_found");

        uow.MemberRole.Remove(memberRoleDb);
        return await uow.Save();
    }
}
