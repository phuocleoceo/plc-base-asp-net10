using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.User.Entities;

namespace PlcBase.Features.User.Repositories;

public class UserAccountRepository(DataContext db, IMapper mapper)
    : BaseRepository<UserAccountEntity>(db, mapper),
        IUserAccountRepository
{
    public async Task<UserAccountEntity> FindByEmail(string email)
    {
        return await GetOneAsync<UserAccountEntity>(
            new QueryModel<UserAccountEntity>() { Filters = { ua => ua.Email == email } }
        );
    }
}
