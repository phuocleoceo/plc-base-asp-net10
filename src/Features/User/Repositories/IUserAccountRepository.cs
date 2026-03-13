using PlcBase.Base.Repository;
using PlcBase.Features.User.Entities;

namespace PlcBase.Features.User.Repositories;

public interface IUserAccountRepository : IBaseRepository<UserAccountEntity>
{
    Task<UserAccountEntity> FindByEmail(string email);
}
