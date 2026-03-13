using System.Security.Claims;
using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.Auth.DTOs;
using PlcBase.Features.User.Entities;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Helpers;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.Auth.Services;

public class AuthService(
    IAuthMailService authMailService,
    IJwtHelper jwtHelper,
    IUnitOfWork uow,
    IMapper mapper
) : IAuthService
{
    public async Task<UserLoginResponseDTO> Login(UserLoginDTO userLoginDTO)
    {
        UserAccountEntity currentUser =
            await uow.UserAccount.GetOneAsync<UserAccountEntity>(
                new QueryModel<UserAccountEntity>()
                {
                    Filters = { u => u.Email == userLoginDTO.Email },
                    Includes = { u => u.Role },
                }
            ) ?? throw new BaseException(HttpCode.NOT_FOUND, "account_not_found");

        if (!currentUser.IsVerified)
            throw new BaseException(HttpCode.BAD_REQUEST, "account_not_verified");

        if (!currentUser.IsActived)
            throw new BaseException(HttpCode.BAD_REQUEST, "account_inactived");

        if (!PasswordUtility.IsValidPassword(userLoginDTO.Password, currentUser.Password))
            throw new BaseException(HttpCode.BAD_REQUEST, "invalid_password");

        IEnumerable<Claim> userClaims = await GetUserClaims(currentUser);
        TokenData accessToken = jwtHelper.CreateAccessToken(userClaims);
        TokenData refreshToken = jwtHelper.CreateRefreshToken(userClaims);

        currentUser.RefreshToken = refreshToken.Token;
        currentUser.RefreshTokenExpiredAt = refreshToken.ExpiredAt;
        uow.UserAccount.Update(currentUser);
        await uow.Save();

        return new UserLoginResponseDTO()
        {
            Id = currentUser.Id,
            Email = currentUser.Email,
            RoleId = currentUser.RoleId,
            RoleName = currentUser.Role.Name,
            AccessToken = accessToken.Token,
            AccessTokenExpiredAt = accessToken.ExpiredAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiredAt = refreshToken.ExpiredAt,
        };
    }

    private async Task<List<Claim>> GetUserClaims(UserAccountEntity user)
    {
        // TODO: Permissions or Role
        List<Claim> claims = new List<Claim>
        {
            new(CustomClaimTypes.UserId, user.Id.ToString()),
            new(CustomClaimTypes.Email, user.Email),
            new(CustomClaimTypes.Role, user.Role.Name),
        };

        await Task.CompletedTask;
        return claims;
    }

    public async Task<UserRefreshTokenResponseDTO> RefreshToken(
        UserRefreshTokenDTO userRefreshTokenDTO
    )
    {
        ClaimsPrincipal principal = jwtHelper.GetPrincipalFromExpiredToken(
            userRefreshTokenDTO.AccessToken
        );

        int userId = Convert.ToInt32(principal.FindFirstValue(CustomClaimTypes.UserId));
        UserAccountEntity currentUser = await uow.UserAccount.FindByIdAsync(userId);

        if (currentUser == null)
            throw new BaseException(HttpCode.NOT_FOUND, "account_has_token_not_found");

        if (
            currentUser.RefreshToken != userRefreshTokenDTO.RefreshToken
            || currentUser.RefreshTokenExpiredAt <= TimeUtility.Now()
        )
            throw new BaseException(HttpCode.BAD_REQUEST, "refresh_token_invalid_or_expired");

        TokenData accessToken = jwtHelper.CreateAccessToken(principal.Claims);
        TokenData refreshToken = jwtHelper.CreateRefreshToken(principal.Claims);

        currentUser.RefreshToken = refreshToken.Token;
        uow.UserAccount.Update(currentUser);
        await uow.Save();

        return new UserRefreshTokenResponseDTO()
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiredAt = accessToken.ExpiredAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiredAt = currentUser.RefreshTokenExpiredAt.Value,
        };
    }

    public async Task<bool> RevokeRefreshToken(ReqUser reqUser)
    {
        UserAccountEntity currentUser = await uow.UserAccount.FindByIdAsync(reqUser.Id);

        if (currentUser == null)
            throw new BaseException(HttpCode.NOT_FOUND, "account_not_found");

        currentUser.RefreshToken = null;
        currentUser.RefreshTokenExpiredAt = null;

        uow.UserAccount.Update(currentUser);
        return await uow.Save();
    }

    public async Task<UserRegisterResponseDTO> Register(UserRegisterDTO userRegisterDTO)
    {
        try
        {
            await uow.CreateTransaction();

            UserAccountEntity newUserAccount = mapper.Map<UserAccountEntity>(userRegisterDTO);
            UserProfileEntity newUserProfile = mapper.Map<UserProfileEntity>(userRegisterDTO);

            // Throw exception if register unique info is exists
            if (await uow.UserAccount.AnyAsync(ua => ua.Email == newUserAccount.Email))
                throw new BaseException(HttpCode.BAD_REQUEST, "email_existed");

            if (
                await uow.UserProfile.AnyAsync(up =>
                    up.PhoneNumber == newUserProfile.PhoneNumber
                    || up.IdentityNumber == newUserProfile.IdentityNumber
                )
            )
                throw new BaseException(
                    HttpCode.BAD_REQUEST,
                    "phone_number_or_identity_number_existed"
                );

            // Create user account
            newUserAccount.Password = PasswordUtility.GetPasswordHash(userRegisterDTO.Password);
            newUserAccount.IsVerified = false;
            newUserAccount.IsActived = true;
            newUserAccount.SecurityCode = CodeSecure.CreateRandomCode();

            uow.UserAccount.Add(newUserAccount);
            await uow.Save();

            // Create user profile with new user account Id
            newUserProfile.CurrentCredit = 0;
            newUserProfile.UserAccountId = newUserAccount.Id;
            uow.UserProfile.Add(newUserProfile);

            await authMailService.SendMailConfirm(newUserAccount, newUserProfile);

            await uow.Save();
            await uow.CommitTransaction();

            return new UserRegisterResponseDTO()
            {
                Id = newUserAccount.Id,
                Email = newUserAccount.Email,
            };
        }
        catch (BaseException)
        {
            await uow.AbortTransaction();
            throw;
        }
    }

    public async Task<bool> ConfirmEmail(UserConfirmEmailDTO userConfirmEmailDTO)
    {
        UserAccountEntity currentUser = await uow.UserAccount.FindByIdAsync(
            userConfirmEmailDTO.UserId
        );

        if (currentUser == null)
            throw new BaseException(HttpCode.NOT_FOUND, "account_not_found");

        if (currentUser.IsVerified)
            throw new BaseException(HttpCode.BAD_REQUEST, "account_already_verified");

        if (currentUser.SecurityCode != userConfirmEmailDTO.Code)
            throw new BaseException(HttpCode.BAD_REQUEST, "security_code_invalid");

        currentUser.IsVerified = true;
        currentUser.SecurityCode = "";

        uow.UserAccount.Update(currentUser);
        return await uow.Save();
    }

    public async Task<bool> ChangePassword(
        ReqUser reqUser,
        UserChangePasswordDTO userChangePasswordDTO
    )
    {
        UserAccountEntity currentUser = await uow.UserAccount.FindByIdAsync(reqUser.Id);

        if (currentUser == null)
            throw new BaseException(HttpCode.NOT_FOUND, "account_not_found");

        if (!currentUser.IsVerified)
            throw new BaseException(HttpCode.BAD_REQUEST, "account_unverified");

        if (
            !PasswordUtility.IsValidPassword(
                userChangePasswordDTO.OldPassword,
                currentUser.Password
            )
        )
            throw new BaseException(HttpCode.BAD_REQUEST, "invalid_old_password");

        currentUser.Password = PasswordUtility.GetPasswordHash(userChangePasswordDTO.NewPassword);

        uow.UserAccount.Update(currentUser);
        return await uow.Save();
    }

    public async Task ForgotPassword(UserForgotPasswordDTO userForgotPasswordDTO)
    {
        string identityInformation = userForgotPasswordDTO.IdentityInformation;

        UserAccountEntity currentUser = await uow.UserAccount.GetOneAsync<UserAccountEntity>(
            new QueryModel<UserAccountEntity>()
            {
                Includes = { ua => ua.UserProfile },
                Filters =
                {
                    ua =>
                        ua.Email == identityInformation
                        || ua.UserProfile.PhoneNumber == identityInformation
                        || ua.UserProfile.IdentityNumber == identityInformation,
                },
            }
        );

        if (currentUser == null)
            throw new BaseException(HttpCode.NOT_FOUND, "account_not_found");

        if (!currentUser.IsVerified)
            throw new BaseException(HttpCode.BAD_REQUEST, "account_unverified");

        string newSecurityCode = CodeSecure.CreateRandomCode();
        currentUser.SecurityCode = newSecurityCode;

        uow.UserAccount.Update(currentUser);
        await uow.Save();

        await authMailService.SendMailRecoverPassword(currentUser, currentUser.UserProfile);
    }

    public async Task<bool> RecoverPassword(UserRecoverPasswordDTO userRecoverPasswordDTO)
    {
        UserAccountEntity currentUser = await uow.UserAccount.FindByIdAsync(
            userRecoverPasswordDTO.UserId
        );

        if (currentUser == null)
            throw new BaseException(HttpCode.NOT_FOUND, "account_not_found");

        if (!currentUser.IsVerified)
            throw new BaseException(HttpCode.BAD_REQUEST, "account_unverified");

        if (currentUser.SecurityCode != userRecoverPasswordDTO.Code)
            throw new BaseException(HttpCode.BAD_REQUEST, "security_code_invalid");

        currentUser.Password = PasswordUtility.GetPasswordHash(userRecoverPasswordDTO.NewPassword);
        currentUser.SecurityCode = "";

        uow.UserAccount.Update(currentUser);
        return await uow.Save();
    }
}
