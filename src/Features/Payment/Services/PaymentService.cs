using AutoMapper;
using Microsoft.Extensions.Options;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Payment.DTOs;
using PlcBase.Features.Payment.Entities;
using PlcBase.Features.User.Entities;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Enums;
using PlcBase.Shared.Helpers;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Payment.Services;

public class PaymentService(IOptions<VNPSettings> vnpSettings, IUnitOfWork uow, IMapper mapper)
    : IPaymentService
{
    private readonly VNPSettings _vnpSettings = vnpSettings.Value;

    public async Task<string> CreatePayment(ReqUser reqUser, CreatePaymentDTO createPaymentDTO)
    {
        try
        {
            await uow.CreateTransaction();

            VNPHistory vnpHistory = new();
            vnpHistory.vnp_TxnRef = TimeUtility.Now().Ticks;
            vnpHistory.vnp_OrderInfo = $"{reqUser.Id}|{vnpHistory.vnp_TxnRef}";
            // Must multiply by 100 to send to vnpay system
            vnpHistory.vnp_Amount = createPaymentDTO.Amount * 100;
            vnpHistory.vnp_TmnCode = _vnpSettings.TmnCode;
            vnpHistory.vnp_CreateDate = TimeUtility.GetDateTimeFormatted(
                TimeUtility.Now(),
                "yyyyMMddHHmmss"
            );

            //Build URL for VNPAY
            VNPLibrary vnpay = new();
            vnpay.AddRequestData("vnp_Version", VNPLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", _vnpSettings.Command);
            vnpay.AddRequestData("vnp_TmnCode", vnpHistory.vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", vnpHistory.vnp_Amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", vnpHistory.vnp_CreateDate);
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_IpAddr", "8.8.8.8");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_OrderInfo", vnpHistory.vnp_OrderInfo);
            vnpay.AddRequestData("vnp_ReturnUrl", _vnpSettings.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", vnpHistory.vnp_TxnRef.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(
                _vnpSettings.BaseUrl,
                _vnpSettings.HashSecret,
                vnpHistory
            );

            PaymentEntity paymentEntity = mapper.Map<PaymentEntity>(vnpHistory);
            paymentEntity.UserId = reqUser.Id;

            uow.Payment.Add(paymentEntity);
            await uow.Save();

            await uow.CommitTransaction();
            return paymentUrl;
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }

    public async Task<bool> SubmitPayment(ReqUser reqUser, SubmitPaymentDTO submitPaymentDTO)
    {
        try
        {
            await uow.CreateTransaction();

            // Check payment status
            if (
                submitPaymentDTO.vnp_ResponseCode != PaymentStatus.VNP_TRANSACTION_STATUS_SUCCESS
                || submitPaymentDTO.vnp_TransactionStatus
                    != PaymentStatus.VNP_TRANSACTION_STATUS_SUCCESS
            )
                throw new BaseException(HttpCode.BAD_REQUEST, "payment_fail");

            // Check signature and update payment metadata
            // PaymentEntity paymentEntity = await _uow.Payment.GetByTxnRef(
            //     reqUser.Id,
            //     submitPaymentDTO.vnp_TxnRef
            // );

            // if (paymentEntity.vnp_SecureHash != submitPaymentDTO.vnp_SecureHash)
            //     throw new BaseException(HttpCode.BAD_REQUEST, "invalid_payment_secure_hash");

            long txnRef = Convert.ToInt64(submitPaymentDTO.vnp_OrderInfo.Split("|")[1]);
            PaymentEntity paymentEntity = await uow.Payment.GetByTxnRef(reqUser.Id, txnRef);

            if (paymentEntity.vnp_TransactionStatus == PaymentStatus.VNP_TRANSACTION_STATUS_SUCCESS)
                throw new BaseException(HttpCode.BAD_REQUEST, "payment_already_handled");

            mapper.Map(submitPaymentDTO, paymentEntity);
            paymentEntity.vnp_TransactionStatus = PaymentStatus.VNP_TRANSACTION_STATUS_SUCCESS;
            paymentEntity.vnp_TxnRef = txnRef;
            uow.Payment.Update(paymentEntity);
            await uow.Save();

            // Update user credit
            UserProfileEntity userProfileDb =
                await uow.UserProfile.GetProfileByAccountId(reqUser.Id)
                ?? throw new BaseException(HttpCode.NOT_FOUND, "user_not_found");
            userProfileDb.CurrentCredit += submitPaymentDTO.vnp_Amount / 100;
            uow.UserProfile.Update(userProfileDb);
            await uow.Save();

            await uow.CommitTransaction();
            return true;
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }
}
