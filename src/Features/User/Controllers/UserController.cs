using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DomainModel;
using PlcBase.Base.DTO;
using PlcBase.Features.User.DTOs;
using PlcBase.Features.User.Services;
using PlcBase.Shared.Enums;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.User.Controllers;

public class UserController(IUserService userService) : BaseController
{
    [HttpGet]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<PagedList<UserDTO>>> GetAllUsers(
        [FromQuery] UserParams userParams
    )
    {
        return HttpContext.Success(await userService.GetAllUsers(userParams));
    }

    [HttpGet("Personal")]
    [Authorize]
    public async Task<SuccessResponse<UserProfilePersonalDTO>> GetUserProfilePersonal()
    {
        ReqUser reqUser = HttpContext.GetRequestUser();
        return HttpContext.Success(await userService.GetUserProfilePersonal(reqUser));
    }

    [HttpPut("Personal")]
    [Authorize]
    public async Task<SuccessResponse<bool>> UpdateUserProfilePersonal(
        [FromBody] UserProfileUpdateDTO userProfileUpdateDTO
    )
    {
        ReqUser reqUser = HttpContext.GetRequestUser();

        if (await userService.UpdateUserProfile(reqUser, userProfileUpdateDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }

    [HttpGet("Anonymous/{userId}")]
    [Authorize]
    public async Task<SuccessResponse<UserProfileAnonymousDTO>> GetUserProfileAnonymous(int userId)
    {
        return HttpContext.Success(await userService.GetUserProfileAnonymous(userId));
    }

    [HttpGet("Account/{userId}")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<UserAccountDTO>> GetUserAccountById(int userId)
    {
        return HttpContext.Success(await userService.GetUserAccountById(userId));
    }

    [HttpPut("Account/{userId}")]
    [Authorize(Roles = AppRole.ADMIN)]
    public async Task<SuccessResponse<bool>> UpdateUserAccount(
        int userId,
        [FromBody] UserAccountUpdateDTO userAccountUpdateDTO
    )
    {
        if (await userService.UpdateUserAccount(userId, userAccountUpdateDTO))
            return HttpContext.Success(true);
        return HttpContext.Failure();
    }
}
