using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.Auth.DTOs;
using PlcBase.Features.Auth.Services;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Auth.Controllers;

public class AuthController(IAuthService authService) : BaseController
{
    [HttpPost("Login")]
    public async Task<SuccessResponse<UserLoginResponseDTO>> Login(
        [FromBody] UserLoginDTO userLoginDTO
    )
    {
        return HttpContext.Success(await authService.Login(userLoginDTO));
    }

    [HttpPost("Register")]
    public async Task<SuccessResponse<UserRegisterResponseDTO>> Register(
        [FromBody] UserRegisterDTO userRegisterDTO
    )
    {
        return HttpContext.Success(await authService.Register(userRegisterDTO));
    }

    [HttpPut("Confirm-Email")]
    public async Task<SuccessResponse<bool>> ConfirmEmail(
        [FromBody] UserConfirmEmailDTO userConfirmEmailDTO
    )
    {
        if (await authService.ConfirmEmail(userConfirmEmailDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPut("Change-Password")]
    [Authorize]
    public async Task<SuccessResponse<bool>> ChangePassword(
        [FromBody] UserChangePasswordDTO userChangePasswordDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await authService.ChangePassword(reqUser, userChangePasswordDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPost("Forgot-Password")]
    public async Task<SuccessResponse<bool>> ForgotPassword(
        [FromBody] UserForgotPasswordDTO userForgotPasswordDTO
    )
    {
        await authService.ForgotPassword(userForgotPasswordDTO);
        return HttpContext.Success(true);
    }

    [HttpPut("Recover-Password")]
    public async Task<SuccessResponse<bool>> RecoverPassword(
        UserRecoverPasswordDTO userRecoverPasswordDTO
    )
    {
        if (await authService.RecoverPassword(userRecoverPasswordDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpPost("Refresh-Token")]
    public async Task<SuccessResponse<UserRefreshTokenResponseDTO>> RefreshToken(
        [FromBody] UserRefreshTokenDTO userRefreshTokenDTO
    )
    {
        return HttpContext.Success(await authService.RefreshToken(userRefreshTokenDTO));
    }

    [HttpPost("Revoke-Refresh-Token")]
    [Authorize]
    public async Task<SuccessResponse<bool>> RevokeRefreshToken()
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await authService.RevokeRefreshToken(reqUser))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
