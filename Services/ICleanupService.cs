using Serilog;

namespace HangfireDemo.Services;

public interface ICleanupService
{
    void CleanupOldLogs();
    void CleanupExpiredSessions();
}

public class CleanupService : ICleanupService
{
    private readonly ILogger<CleanupService> _logger;

    public CleanupService(ILogger<CleanupService> logger)
    {
        _logger = logger;
    }

    public void CleanupOldLogs()
    {
        var cutoffDate = DateTime.Now.AddDays(-7);

        Log.ForContext("CutoffDate", cutoffDate)
           .ForContext("JobType", "CleanupOldLogs")
           .Information("Starting cleanup of logs older than {CutoffDate}", cutoffDate);

        // Simulate cleanup operation
        Thread.Sleep(3000);

        var deletedCount = new Random().Next(10, 100);

        Log.ForContext("DeletedCount", deletedCount)
           .ForContext("JobType", "CleanupOldLogs")
           .Information("Cleanup completed. Deleted {DeletedCount} log entries", deletedCount);
    }

    public void CleanupExpiredSessions()
    {
        Log.Information("Starting cleanup of expired sessions");
        Thread.Sleep(2000);

        var deletedCount = new Random().Next(5, 50);
        Log.Information("Cleanup completed. Deleted {DeletedCount} expired sessions", deletedCount);
    }
}
