namespace HangfireDemo.Models;

public class EmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? DelayMinutes { get; set; }
}

public class TodoRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime RemindAt { get; set; }
}

public class ReportRequest
{
    public int ReportId { get; set; }
    public string Email { get; set; } = string.Empty;
}
