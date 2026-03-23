using PlcBase.Common.Filters;
using PlcBase.Common.Repositories;
using PlcBase.Common.Services;
using PlcBase.Shared.Helpers;

namespace PlcBase.Extensions.ServiceCollections;

public static class DIExtension
{
    public static void ConfigureDataFactory(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddAutoMapper(_ => { }, typeof(Program).Assembly);
        services.ConfigureAppSetting(configuration);
        services.ConfigureHelperDI();
        services.ConfigureFilterDI();
        services.ConfigureRepositoryDI();
        services.ConfigureServiceDI();
    }
}
