using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.ConfigSetting.Entities;

namespace PlcBase.Features.ConfigSetting.Repositories;

public class ConfigSettingRepository(DataContext db, IMapper mapper)
    : BaseRepository<ConfigSettingEntity>(db, mapper),
        IConfigSettingRepository
{
    public async Task<ConfigSettingEntity> GetByKey(string key)
    {
        return await GetOneAsync<ConfigSettingEntity>(
            new QueryModel<ConfigSettingEntity>() { Filters = { cf => cf.Key == key } }
        );
    }

    public async Task<double> GetValueByKey(string key)
    {
        return (await GetByKey(key)).Value;
    }
}
