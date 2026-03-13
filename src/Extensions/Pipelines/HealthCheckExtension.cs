using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace PlcBase.Extensions.Pipelines;

public static class HealthCheckExtension
{
    public static void UseHealthCheck(this WebApplication app)
    {
        app.MapHealthChecks(
            "/health",
            new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
        );
    }
}
