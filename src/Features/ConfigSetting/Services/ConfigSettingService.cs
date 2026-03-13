using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Error;
using PlcBase.Common.Repositories;
using PlcBase.Features.ConfigSetting.DTOs;
using PlcBase.Features.ConfigSetting.Entities;
using PlcBase.Shared.Constants;
using PlcBase.Shared.Helpers;
using PlcBase.Shared.Utilities;

namespace PlcBase.Features.ConfigSetting.Services;

public class ConfigSettingService(IUnitOfWork uow, IMapper mapper, IRedisHelper redisHelper)
    : IConfigSettingService
{
    public async Task<List<ConfigSettingDTO>> GetAllConfigSettings()
    {
        return await redisHelper.GetCachedOr(
            RedisUtility.GetListKey<ConfigSettingDTO>(),
            async () => await uow.ConfigSetting.GetManyAsync<ConfigSettingDTO>()
        );
    }

    public async Task<ConfigSettingDTO> GetByKey(string key)
    {
        return await redisHelper.GetCachedOr(
            RedisUtility.GetKey<ConfigSettingDTO>(key),
            async () =>
                await uow.ConfigSetting.GetOneAsync<ConfigSettingDTO>(
                    new QueryModel<ConfigSettingEntity>() { Filters = { cf => cf.Key == key } }
                )
        );
    }

    public async Task<bool> UpdateForKey(string key, ConfigSettingUpdateDTO configSettingUpdateDTO)
    {
        ConfigSettingEntity configSettingDb = await uow.ConfigSetting.GetByKey(key);
        if (configSettingDb == null)
            throw new BaseException(HttpCode.NOT_FOUND, "config_setting_not_found");

        mapper.Map(configSettingUpdateDTO, configSettingDb);
        uow.ConfigSetting.Update(configSettingDb);

        await redisHelper.ClearByPattern(RedisUtility.GetClearKey<ConfigSettingDTO>());
        return await uow.Save();
    }
}
