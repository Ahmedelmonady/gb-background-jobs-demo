namespace HangfireDemo.Services;

public interface IReportService
{
    string GenerateReport(int reportId);
    void SendReportNotification(string fileName, string email);
}

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;
    }

    public string GenerateReport(int reportId)
    {
        _logger.LogInformation("Starting report generation for Report ID: {ReportId}", reportId);

        // Simulate long-running report generation
        Thread.Sleep(5000);

        var fileName = $"report_{reportId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
        var csv = "Id,Name,Value,Date\n1,Product A,100,2024-01-01\n2,Product B,200,2024-01-02";

        // In real app, save to file system or blob storage
        _logger.LogInformation("Report generated successfully: {FileName}", fileName);

        return fileName;
    }

    public void SendReportNotification(string fileName, string email)
    {
        _logger.LogInformation("Sending report notification for {FileName} to {Email}", fileName, email);
        Thread.Sleep(1000);
        _logger.LogInformation("Report notification sent successfully");
    }
}
