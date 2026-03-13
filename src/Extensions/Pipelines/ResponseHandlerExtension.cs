using PlcBase.Middlewares;
using PlcBase.Shared.Helpers;

namespace PlcBase.Extensions.Pipelines;

public static class ResponseHandlerExtension
{
    public static void UseResponseHandlerPipeline(this WebApplication app, ILoggerManager logger)
    {
        app.ConfigureSuccessHandler(logger);

        app.ConfigureErrorHandler(logger);

        app.UseCustomAuthResponse();
    }
}
