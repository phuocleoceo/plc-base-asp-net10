using PlcBase.Base.Repository;
using PlcBase.Features.User.Entities;

namespace PlcBase.Features.User.Repositories;

public interface IUserProfileRepository : IBaseRepository<UserProfileEntity>
{
    Task<UserProfileEntity> GetProfileByAccountId(int accountId);

    Task<bool> MakePayment(int userId, double amount);
}
