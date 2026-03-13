using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.Payment.DTOs;
using PlcBase.Features.Payment.Services;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Payment.Controllers;

public class PaymentController(IPaymentService paymentService) : BaseController
{
    [HttpPost]
    [Authorize]
    public async Task<SuccessResponse<string>> CreatePayment(
        [FromBody] CreatePaymentDTO createPaymentDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();
        return HttpContext.Success(await paymentService.CreatePayment(reqUser, createPaymentDTO));
    }

    [HttpPut]
    [Authorize]
    public async Task<SuccessResponse<bool>> SubmitPayment(
        [FromBody] SubmitPaymentDTO submitPaymentDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await paymentService.SubmitPayment(reqUser, submitPaymentDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
