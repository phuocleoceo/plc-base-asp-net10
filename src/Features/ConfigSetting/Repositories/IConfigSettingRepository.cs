using PlcBase.Base.Repository;
using PlcBase.Features.ConfigSetting.Entities;

namespace PlcBase.Features.ConfigSetting.Repositories;

public interface IConfigSettingRepository : IBaseRepository<ConfigSettingEntity>
{
    Task<ConfigSettingEntity> GetByKey(string key);

    Task<double> GetValueByKey(string key);
}
