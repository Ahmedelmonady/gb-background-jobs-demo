# Hangfire Quick Reference Cheat Sheet

**Quick reference for background jobs in .NET**

---

## 📦 Installation

```bash
dotnet add package Hangfire.Core
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.InMemory          # For development
dotnet add package Hangfire.PostgreSql        # For production
```

---

## ⚙️ Configuration

### Program.cs

```csharp
using Hangfire;

// Add Hangfire services
builder.Services.AddHangfire(config => config
    .UseInMemoryStorage());  // Development
    // .UsePostgreSqlStorage(...)  // Production

// Add Hangfire server
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.Queues = new[] { "critical", "default" };
});

// Dashboard
app.UseHangfireDashboard("/hangfire");
```

---

## 🔥 Job Types

### 1. Fire-and-Forget (Immediate)
**Run once, ASAP**

```csharp
// Simple
BackgroundJob.Enqueue(() => Console.WriteLine("Hello!"));

// With service
BackgroundJob.Enqueue<IEmailService>(x => x.SendEmail(email));

// Returns job ID
var jobId = BackgroundJob.Enqueue(() => ProcessOrder(orderId));
```

**Use cases**: Welcome emails, file uploads, notifications

---

### 2. Delayed Jobs (Scheduled)
**Run once, at specific time or after delay**

```csharp
// Delay by TimeSpan
BackgroundJob.Schedule(() => SendReminder(), TimeSpan.FromHours(24));

// Delay by DateTime
BackgroundJob.Schedule(() => SendEmail(), new DateTime(2024, 12, 31, 23, 59, 0));

// With service
BackgroundJob.Schedule<IEmailService>(
    x => x.SendEmail(email),
    TimeSpan.FromMinutes(30)
);
```

**Use cases**: Scheduled campaigns, trial expiry, delayed notifications

---

### 3. Recurring Jobs
**Run on schedule (cron)**

```csharp
// Add or update recurring job
RecurringJob.AddOrUpdate(
    "job-id",                           // Unique identifier
    () => CleanupOldData(),             // Job to execute
    Cron.Daily(2, 0)                    // Schedule (2 AM daily)
);

// With service
RecurringJob.AddOrUpdate<ICleanupService>(
    "daily-cleanup",
    x => x.CleanupLogs(),
    Cron.Daily()
);

// Trigger manually
RecurringJob.Trigger("job-id");

// Remove
RecurringJob.RemoveIfExists("job-id");
```

**Use cases**: Daily reports, data cleanup, synchronization

---

### 4. Continuations (Job Chains)
**Run after another job completes**

```csharp
// Simple chain
var job1 = BackgroundJob.Enqueue(() => GenerateReport());
BackgroundJob.ContinueJobWith(job1, () => SendEmail());

// Multiple steps
var job1 = BackgroundJob.Enqueue(() => Step1());
var job2 = BackgroundJob.ContinueJobWith(job1, () => Step2());
var job3 = BackgroundJob.ContinueJobWith(job2, () => Step3());

// With service
var reportJob = BackgroundJob.Enqueue<IReportService>(x => x.Generate());
BackgroundJob.ContinueJobWith<IEmailService>(
    reportJob,
    x => x.SendNotification()
);
```

**Use cases**: Multi-step workflows, process → notify

---

## 📅 Cron Expressions

```csharp
Cron.Minutely()                         // Every minute
Cron.Hourly()                           // Every hour
Cron.Daily()                            // Midnight daily
Cron.Daily(hour)                        // Daily at specific hour
Cron.Daily(hour, minute)                // Daily at specific time
Cron.Weekly()                           // Sunday midnight
Cron.Weekly(DayOfWeek.Monday)           // Monday midnight
Cron.Weekly(DayOfWeek.Monday, hour)     // Monday at specific hour
Cron.Monthly()                          // 1st of month
Cron.Yearly()                           // Jan 1st

// Custom expressions
"*/5 * * * *"      // Every 5 minutes
"0 */2 * * *"      // Every 2 hours
"0 9-17 * * *"     // Every hour from 9 AM to 5 PM
"0 0 * * 1-5"      // Midnight, Monday to Friday
"30 3 * * *"       // 3:30 AM daily
"0 0 1 * *"        // Midnight on 1st of every month
```

**Cron helper**: https://crontab.guru

---

## 🎯 Job Management

```csharp
// Delete/cancel job
BackgroundJob.Delete(jobId);

// Requeue failed job
BackgroundJob.Requeue(jobId);

// Get job state
var monitor = JobStorage.Current.GetMonitoringApi();
var jobDetails = monitor.JobDetails(jobId);
```

---

## 🔄 Retry & Error Handling

### Automatic Retry Attribute

```csharp
// Default: 3 attempts with exponential backoff
[AutomaticRetry(Attempts = 3)]
public void ProcessPayment() { }

// No retry
[AutomaticRetry(Attempts = 0)]
public void SendEmail() { }

// Custom retry
[AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public void ImportData() { }

// Retry with delay
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
public void SyncData() { }
```

### Manual Error Handling

```csharp
public void ProcessOrder(int orderId)
{
    try
    {
        // Job logic
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing order {OrderId}", orderId);
        throw;  // Re-throw to let Hangfire retry
    }
}
```

---

## 🏷️ Job Attributes

```csharp
// Display name in dashboard
[DisplayName("Send Welcome Email to {0}")]
public void SendEmail(string email) { }

// Disable concurrent execution
[DisableConcurrentExecution(timeoutInSeconds: 60)]
public void ProcessBatch() { }

// Queue
[Queue("critical")]
public void ProcessPayment() { }
```

---

## 🎨 Best Practices

### ✅ DO

```csharp
// 1. Use descriptive names
[DisplayName("Process Order #{0}")]
public void ProcessOrder(int orderId) { }

// 2. Pass simple parameters
public void SendEmail(int userId, string template) { }

// 3. Make idempotent
public void UpdatePlayer(int playerId)
{
    if (AlreadyProcessed(playerId)) return;
    // Process
}

// 4. Use dependency injection
BackgroundJob.Enqueue<IEmailService>(x => x.Send(email));

// 5. Add logging
_logger.LogInformation("Processing order {OrderId}", orderId);

// 6. Handle errors
try { } catch (Exception ex) { _logger.LogError(ex, "Error"); throw; }
```

### ❌ DON'T

```csharp
// 1. Don't pass complex objects
BackgroundJob.Enqueue(() => Process(dbContext));  // ❌ Context disposed

// 2. Don't use generic names
public void DoWork() { }  // ❌ Not descriptive

// 3. Don't create huge jobs
public void ProcessEverything() { }  // ❌ Break into smaller jobs

// 4. Don't ignore errors
public void Process() { /* No error handling */ }  // ❌
```

---

## 🎛️ Dashboard

**Access**: `https://localhost:7000/hangfire`

### Tabs
- **Jobs**: All jobs (enqueued, processing, succeeded, failed)
- **Recurring Jobs**: Manage recurring jobs
- **Servers**: Active Hangfire servers
- **Retries**: Failed jobs waiting for retry

### Secure Dashboard

```csharp
public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthFilter() }
});
```

---

## 📊 Monitoring

```csharp
var monitor = JobStorage.Current.GetMonitoringApi();

// Statistics
var stats = monitor.GetStatistics();
Console.WriteLine($"Enqueued: {stats.Enqueued}");
Console.WriteLine($"Processing: {stats.Processing}");
Console.WriteLine($"Succeeded: {stats.Succeeded}");
Console.WriteLine($"Failed: {stats.Failed}");

// Processing jobs
var processing = monitor.ProcessingJobs(0, 10);

// Failed jobs
var failed = monitor.FailedJobs(0, 10);

// Job details
var details = monitor.JobDetails(jobId);
```

---

## 🔧 Configuration Options

### Server Options

```csharp
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 10;                    // Number of parallel jobs
    options.Queues = new[] { "critical", "default" };  // Queue priority
    options.ServerName = "MyServer";             // Server name
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
    options.ShutdownTimeout = TimeSpan.FromMinutes(5);
});
```

### Storage Options

```csharp
// InMemory (Development)
config.UseInMemoryStorage();

// PostgreSQL (Production)
config.UsePostgreSqlStorage(options =>
    options.UseNpgsqlConnection(connectionString),
    new PostgreSqlStorageOptions
    {
        PrepareSchemaIfNecessary = true,
        QueuePollInterval = TimeSpan.FromSeconds(1)
    });

// SQL Server (Production)
config.UseSqlServerStorage(connectionString);

// Redis (High-throughput)
config.UseRedisStorage(connectionString);
```

---

## 🚀 Common Patterns

### Pattern 1: Service with Jobs

```csharp
public interface IOrderService
{
    void ProcessOrder(int orderId);
}

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    [DisplayName("Process Order #{0}")]
    public void ProcessOrder(int orderId)
    {
        try
        {
            _logger.LogInformation("Processing order {OrderId}", orderId);
            // Business logic
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", orderId);
            throw;
        }
    }
}

// Usage in controller
[HttpPost("process/{orderId}")]
public IActionResult ProcessOrder(int orderId)
{
    var jobId = BackgroundJob.Enqueue<IOrderService>(x => x.ProcessOrder(orderId));
    return Accepted(new { jobId });
}
```

---

### Pattern 2: Job with Result Storage

```csharp
public class ReportService
{
    public async Task<string> GenerateReport(int reportId)
    {
        // Generate report
        var fileName = $"report_{reportId}.csv";

        // Save to storage
        await _blobService.Upload(fileName, data);

        return fileName;
    }
}

// Enqueue and continuation
var reportJob = BackgroundJob.Enqueue<IReportService>(x => x.GenerateReport(123));
BackgroundJob.ContinueJobWith<IEmailService>(
    reportJob,
    x => x.SendReportEmail("report_123.csv", "user@example.com")
);
```

---

### Pattern 3: Batch Processing

```csharp
public class BatchService
{
    public void ProcessBatch(List<int> userIds)
    {
        foreach (var userId in userIds)
        {
            try
            {
                ProcessUser(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user {UserId}", userId);
                // Continue with next user
            }
        }
    }
}

// Usage
var userIds = await _dbContext.Users.Select(u => u.Id).ToListAsync();
BackgroundJob.Enqueue<IBatchService>(x => x.ProcessBatch(userIds));
```

---

## 🔍 Troubleshooting

| Problem | Solution |
|---------|----------|
| Jobs not executing | Check `AddHangfireServer()` is called |
| Jobs failing silently | Add error handling and logging |
| Dashboard 401 | Check authorization filter |
| Jobs executing twice | Ensure unique server names |
| Slow processing | Increase `WorkerCount` |
| Jobs disappear on restart | Use persistent storage (PostgreSQL) |
| Memory issues | Use persistent storage, not InMemory |

---

## 📚 Resources

- **Documentation**: https://docs.hangfire.io
- **Cron Generator**: https://crontab.guru
- **GitHub**: https://github.com/HangfireIO/Hangfire
- **NuGet**: https://www.nuget.org/packages/Hangfire.Core

---

## 💡 When to Use Background Jobs

### ✅ Use Background Jobs For:
- Operations > 5 seconds
- Tasks that can be delayed
- Scheduled/recurring operations
- CPU-intensive calculations
- External API calls
- Batch processing
- Email/SMS sending
- Report generation
- Data synchronization

### ❌ Don't Use Background Jobs For:
- Fast operations (< 1 second)
- Operations requiring immediate feedback
- Real-time updates (use SignalR)
- Very high-frequency operations (> 1000/sec)

---

## 🎯 Quick Decision Tree

```
Is operation > 5 seconds?
├─ Yes → Can it be delayed?
│  ├─ Yes → Use Background Job ✅
│  └─ No → Optimize or use async/await
└─ No → Run synchronously

Which job type?
├─ Run once, now? → Fire-and-Forget
├─ Run once, later? → Delayed
├─ Run on schedule? → Recurring
└─ Run after another job? → Continuation
```

---

**Keep this handy while coding! 📌**
