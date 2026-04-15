using Microsoft.Extensions.Options;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Options;

namespace Ptcent.Cloud.Drive.Web.Services
{
    /// <summary>
    /// 回收站自动清理后台服务
    /// </summary>
    public class RecycleBinCleanupHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<RecycleBinCleanupOptions> _options;
        private readonly ILogger<RecycleBinCleanupHostedService> _logger;

        public RecycleBinCleanupHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<RecycleBinCleanupOptions> options,
            ILogger<RecycleBinCleanupHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Value.Enabled)
            {
                _logger.LogInformation("Recycle bin cleanup hosted service is disabled.");
                return;
            }

            await ExecuteCleanupAsync(stoppingToken);

            var intervalHours = Math.Max(1, _options.Value.IntervalHours);
            using var timer = new PeriodicTimer(TimeSpan.FromHours(intervalHours));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecuteCleanupAsync(stoppingToken);
            }
        }

        private async Task ExecuteCleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<IRecycleBinCleanupService>();
                var deletedCount = await cleanupService.CleanupExpiredAsync(cancellationToken);
                _logger.LogInformation("Recycle bin cleanup finished. DeletedCount={DeletedCount}", deletedCount);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recycle bin cleanup failed.");
            }
        }
    }
}
