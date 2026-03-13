using PlcBase.Base.DomainModel;
using PlcBase.Features.Payment.DTOs;

namespace PlcBase.Features.Payment.Services;

public interface IPaymentService
{
    Task<string> CreatePayment(ReqUser reqUser, CreatePaymentDTO createPaymentDTO);

    Task<bool> SubmitPayment(ReqUser reqUser, SubmitPaymentDTO submitPaymentDTO);
}
