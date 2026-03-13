using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.User.Entities;

namespace PlcBase.Features.User.Repositories;

public class UserProfileRepository(DataContext db, IMapper mapper)
    : BaseRepository<UserProfileEntity>(db, mapper),
        IUserProfileRepository
{
    public async Task<UserProfileEntity> GetProfileByAccountId(int accountId)
    {
        return await GetOneAsync<UserProfileEntity>(
            new QueryModel<UserProfileEntity>()
            {
                Filters = { up => up.UserAccountId == accountId },
            }
        );
    }

    public async Task<bool> MakePayment(int userId, double amount)
    {
        UserProfileEntity userProfileDb = await GetProfileByAccountId(userId);
        if (userProfileDb.CurrentCredit < amount)
            return false;

        userProfileDb.CurrentCredit -= amount;
        Update(userProfileDb);
        return true;
    }
}
