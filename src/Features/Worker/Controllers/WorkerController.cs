using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Shared.Enums;
using PlcBase.Shared.Helpers;

namespace PlcBase.Features.Worker.Controllers;

public class WorkerController(ISendMailHelper sendMailHelper) : BaseController
{
    [NonAction]
    [CapSubscribe(WorkerType.SEND_MAIL)]
    public async Task SendMail(MailContent mailContent)
    {
        await sendMailHelper.SendEmailAsync(mailContent);
    }
}
