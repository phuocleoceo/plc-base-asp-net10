using PlcBase.Base.Repository;
using PlcBase.Features.Payment.Entities;

namespace PlcBase.Features.Payment.Repositories;

public interface IPaymentRepository : IBaseRepository<PaymentEntity>
{
    Task<PaymentEntity> GetByTxnRef(int userId, long txnRef);
}
