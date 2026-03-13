using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Invitation.DTOs;
using PlcBase.Features.Invitation.Entities;
using PlcBase.Features.ProjectMember.Entities;
using PlcBase.Features.User.Entities;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Invitation.Services;

public class InvitationService(IUnitOfWork uow) : IInvitationService
{
    public async Task<PagedList<RecipientInvitationDTO>> GetInvitationsForUser(
        ReqUser reqUser,
        RecipientInvitationParams recipientInvitationParams
    )
    {
        QueryModel<InvitationEntity> invitationQuery = new()
        {
            OrderBy = c => c.OrderByDescending(up => up.CreatedAt),
            Filters = { i => i.RecipientId == reqUser.Id },
            Includes = { i => i.Project, i => i.Sender.UserProfile },
            PageSize = recipientInvitationParams.PageSize,
            PageNumber = recipientInvitationParams.PageNumber,
        };

        if (recipientInvitationParams.StillValid)
            invitationQuery.Filters.Add(i => i.AcceptedAt == null && i.DeclinedAt == null);

        if (!string.IsNullOrWhiteSpace(recipientInvitationParams.SearchValue))
        {
            string searchValue = recipientInvitationParams.SearchValue.ToLower();
            invitationQuery.Filters.Add(i =>
                i.Sender.Email.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase)
                || i.Sender.UserProfile.DisplayName.Contains(
                    searchValue,
                    StringComparison.CurrentCultureIgnoreCase
                )
                || i.Project.Name.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase)
                || i.Project.Key.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        return await uow.Invitation.GetPagedAsync<RecipientInvitationDTO>(invitationQuery);
    }

    public async Task<PagedList<SenderInvitationDTO>> GetInvitationsForProject(
        int projectId,
        SenderInvitationParams senderInvitationParams
    )
    {
        QueryModel<InvitationEntity> invitationQuery = new()
        {
            OrderBy = c => c.OrderByDescending(up => up.CreatedAt),
            Filters = { i => i.ProjectId == projectId },
            Includes = { i => i.Recipient.UserProfile },
            PageSize = senderInvitationParams.PageSize,
            PageNumber = senderInvitationParams.PageNumber,
        };

        if (senderInvitationParams.StillValid)
            invitationQuery.Filters.Add(i => i.AcceptedAt == null && i.DeclinedAt == null);

        if (!string.IsNullOrWhiteSpace(senderInvitationParams.SearchValue))
        {
            string searchValue = senderInvitationParams.SearchValue.ToLower();
            invitationQuery.Filters.Add(i =>
                i.Recipient.Email.Contains(searchValue, StringComparison.CurrentCultureIgnoreCase)
                || i.Recipient.UserProfile.DisplayName.Contains(
                    searchValue,
                    StringComparison.CurrentCultureIgnoreCase
                )
            );
        }

        return await uow.Invitation.GetPagedAsync<SenderInvitationDTO>(invitationQuery);
    }

    public async Task<bool> CreateInvitaion(
        ReqUser reqUser,
        int projectId,
        CreateInvitationDTO createInvitationDTO
    )
    {
        UserAccountEntity userAccountDb =
            await uow.UserAccount.FindByEmail(createInvitationDTO.RecipientEmail)
            ?? throw new BaseException(HttpCode.NOT_FOUND, "recipient_not_found");

        if (reqUser.Id == userAccountDb.Id)
            throw new BaseException(HttpCode.BAD_REQUEST, "invalid_invitation");

        InvitationEntity invitationEntity = new()
        {
            ProjectId = projectId,
            SenderId = reqUser.Id,
            RecipientId = userAccountDb.Id,
        };

        uow.Invitation.Add(invitationEntity);
        return await uow.Save();
    }

    public async Task<bool> DeleteInvitation(ReqUser reqUser, int projectId, int invitationId)
    {
        InvitationEntity invitationDb = await uow.Invitation.FindByIdAsync(invitationId);

        if (invitationDb.ProjectId != projectId || invitationDb.SenderId != reqUser.Id)
            throw new BaseException(HttpCode.BAD_REQUEST, "invalid_invitation");

        uow.Invitation.Remove(invitationDb);
        return await uow.Save();
    }

    public async Task<bool> AcceptInvitation(ReqUser reqUser, int invitationId)
    {
        InvitationEntity invitationDb = await uow.Invitation.FindByIdAsync(invitationId);

        if (invitationDb.RecipientId != reqUser.Id)
            throw new BaseException(HttpCode.BAD_REQUEST, "invalid_invitation");

        if (invitationDb.AcceptedAt != null || invitationDb.DeclinedAt != null)
            throw new BaseException(HttpCode.BAD_REQUEST, "completed_invitation");

        invitationDb.AcceptedAt = TimeUtility.Now();
        invitationDb.DeclinedAt = null;
        uow.Invitation.Update(invitationDb);

        uow.ProjectMember.Add(
            new ProjectMemberEntity()
            {
                UserId = invitationDb.RecipientId,
                ProjectId = invitationDb.ProjectId,
            }
        );

        return await uow.Save();
    }

    public async Task<bool> DeclineInvitation(ReqUser reqUser, int invitationId)
    {
        InvitationEntity invitationDb = await uow.Invitation.FindByIdAsync(invitationId);

        if (invitationDb.RecipientId != reqUser.Id)
            throw new BaseException(HttpCode.BAD_REQUEST, "invalid_invitation");

        if (invitationDb.AcceptedAt != null || invitationDb.DeclinedAt != null)
            throw new BaseException(HttpCode.BAD_REQUEST, "completed_invitation");

        invitationDb.AcceptedAt = null;
        invitationDb.DeclinedAt = TimeUtility.Now();
        uow.Invitation.Update(invitationDb);
        return await uow.Save();
    }
}
