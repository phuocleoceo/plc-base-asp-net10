using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.Payment.Entities;

namespace PlcBase.Features.Payment.Repositories;

public class PaymentRepository(DataContext db, IMapper mapper)
    : BaseRepository<PaymentEntity>(db, mapper),
        IPaymentRepository
{
    public async Task<PaymentEntity> GetByTxnRef(int userId, long txnRef)
    {
        return await GetOneAsync<PaymentEntity>(
            new QueryModel<PaymentEntity>()
            {
                Filters = { p => p.UserId == userId && p.vnp_TxnRef == txnRef },
            }
        );
    }
}
