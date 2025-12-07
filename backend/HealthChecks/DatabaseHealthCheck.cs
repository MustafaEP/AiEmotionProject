using backend.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace backend.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _dbContext;

        public DatabaseHealthCheck(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database bağlantısı başarılı.");
                }
                return HealthCheckResult.Unhealthy("Database bağlantısı başarısız.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database kontrolü sırasında hata oluştu.", ex);
            }
        }
    }
}

