# Hangfire Demo Project

Complete demo project for Hangfire presentation with live examples and hands-on exercises.

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Any IDE (Visual Studio, VS Code, Rider)

### Run the Application

```bash
cd HangfireDemo
dotnet restore
dotnet run
```

### Access the Application

- **API**: https://localhost:7000 or http://localhost:5000
- **Swagger UI**: https://localhost:7000/swagger
- **Hangfire Dashboard**: https://localhost:7000/hangfire

## 📚 Project Structure

```
HangfireDemo/
├── Controllers/
│   ├── DemoController.cs          # Live demo endpoints
│   └── ExercisesController.cs     # Hands-on exercise starters
├── Services/
│   ├── IEmailService.cs           # Email service
│   ├── IReportService.cs          # Report generation
│   ├── ICleanupService.cs         # Cleanup jobs
│   └── ITodoService.cs            # Todo reminders
├── Models/
│   └── EmailRequest.cs            # Request DTOs
├── Program.cs                      # Hangfire configuration
└── appsettings.json               # App settings
```

## 🎯 Live Demos

### Demo 1: Fire-and-Forget Job

**Endpoint**: `POST /api/demo/fire-and-forget`

**Example**:
```bash
curl -X POST https://localhost:7000/api/demo/fire-and-forget \
  -H "Content-Type: application/json" \
  -d '{
    "to": "user@example.com",
    "subject": "Welcome!",
    "body": "Thanks for signing up"
  }'
```

**What it does**: Immediately enqueues an email job that runs in the background.

---

### Demo 2: Delayed Job

**Endpoint**: `POST /api/demo/delayed`

**Example**:
```bash
curl -X POST https://localhost:7000/api/demo/delayed \
  -H "Content-Type: application/json" \
  -d '{
    "to": "user@example.com",
    "subject": "Reminder",
    "body": "Your trial expires soon",
    "delayMinutes": 2
  }'
```

**What it does**: Schedules an email to be sent in 2 minutes.

---

### Demo 3: Recurring Job

**Start**: `POST /api/demo/recurring/start`
**Stop**: `POST /api/demo/recurring/stop`

**Example**:
```bash
# Start recurring job (runs every minute)
curl -X POST https://localhost:7000/api/demo/recurring/start

# Stop recurring job
curl -X POST https://localhost:7000/api/demo/recurring/stop
```

**What it does**: Creates a job that runs every minute to clean up expired sessions.

---

### Demo 4: Job Continuation (Chaining)

**Endpoint**: `POST /api/demo/continuation`

**Example**:
```bash
curl -X POST https://localhost:7000/api/demo/continuation \
  -H "Content-Type: application/json" \
  -d '{
    "reportId": 123,
    "email": "admin@example.com"
  }'
```

**What it does**:
1. First job: Generates a report (takes 5 seconds)
2. Second job: Sends email notification (automatically runs after first job completes)

---

### Demo 5: Complex Chain

**Endpoint**: `POST /api/demo/complex-chain`

**What it does**: Creates a 3-step job chain:
1. Import data
2. Validate data
3. Send notification

---

### Demo 6: Queue Priority

**Endpoint**: `POST /api/demo/queue-priority`

**What it does**: Demonstrates how critical jobs are processed before normal jobs.

---

### Demo 7: Cancel Job

**Endpoint**: `DELETE /api/demo/cancel/{jobId}`

**Example**:
```bash
curl -X DELETE https://localhost:7000/api/demo/cancel/123abc
```

**What it does**: Cancels a scheduled job before it executes.

---

## 🏋️ Hands-On Exercises

All exercises are in the **ExercisesController.cs** file with TODO comments.

### Exercise 1: Todo Reminder System (20 min)

**Goal**: Schedule background jobs to send todo reminders.

**Endpoints**:
- `POST /api/exercises/exercise1/todo` - Create todo with reminder
- `POST /api/exercises/exercise1/recurring-reminder` - Recurring hourly reminder

**Tasks**:
1. Open `ExercisesController.cs`
2. Find the `CreateTodoWithReminder` method
3. Complete the TODO to schedule a job at a specific datetime
4. Test with Swagger or curl:

```bash
curl -X POST https://localhost:7000/api/exercises/exercise1/todo \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "title": "Complete Hangfire exercises",
    "remindAt": "2024-12-31T14:30:00"
  }'
```

**Solution**: Uncomment the SOLUTION section in the code.

**Bonus**: Implement recurring reminders that trigger every hour.

---

### Exercise 2: Report Generation Service (20 min)

**Goal**: Generate reports in background and send email when complete.

**Endpoint**: `POST /api/exercises/exercise2/generate-report`

**Tasks**:
1. Open `ExercisesController.cs`
2. Find the `GenerateReportAsync` method
3. Use fire-and-forget to start report generation
4. Chain an email notification job using continuation
5. Test:

```bash
curl -X POST https://localhost:7000/api/exercises/exercise2/generate-report \
  -H "Content-Type: application/json" \
  -d '{
    "reportId": 456,
    "email": "admin@example.com"
  }'
```

**Expected behavior**:
- First job runs (takes 5 seconds)
- Second job runs automatically after first completes
- View both jobs in the dashboard

**Solution**: Uncomment the SOLUTION section.

---

### Exercise 3: Data Cleanup Job (20 min)

**Goal**: Create a recurring job to clean old data.

**Endpoints**:
- `POST /api/exercises/exercise3/start-cleanup` - Start daily cleanup
- `POST /api/exercises/exercise3/trigger-now` - Trigger manually
- `POST /api/exercises/exercise3/stop-cleanup` - Stop cleanup

**Tasks**:
1. Open `ExercisesController.cs`
2. Find the `StartDailyCleanup` method
3. Create a recurring job that runs daily at 3 AM
4. Implement manual trigger for testing
5. Test:

```bash
# Start daily cleanup
curl -X POST https://localhost:7000/api/exercises/exercise3/start-cleanup

# Trigger immediately (for testing)
curl -X POST https://localhost:7000/api/exercises/exercise3/trigger-now

# Stop cleanup
curl -X POST https://localhost:7000/api/exercises/exercise3/stop-cleanup
```

**Hints**:
- Use `Cron.Daily(3, 0)` for 3 AM schedule
- Use `RecurringJob.Trigger("job-id")` to trigger manually
- Use `RecurringJob.RemoveIfExists("job-id")` to stop

**Bonus**: Add error handling with `[AutomaticRetry(Attempts = 3)]` attribute.

**Solution**: Uncomment the SOLUTION sections.

---

## 🎛️ Hangfire Dashboard

Access the dashboard at: **https://localhost:7000/hangfire**

### What you can do:
- ✅ View all jobs (Enqueued, Processing, Succeeded, Failed)
- ✅ Monitor recurring jobs
- ✅ Manually trigger recurring jobs
- ✅ View job details and parameters
- ✅ See job execution history
- ✅ Retry failed jobs
- ✅ Delete jobs

### Dashboard Tabs:

1. **Jobs** - View all job states
2. **Recurring Jobs** - Manage recurring jobs
3. **Servers** - View Hangfire servers
4. **Retries** - View failed jobs waiting for retry

---

## 📖 Hangfire Basics Recap

### Job Types

```csharp
// 1. Fire-and-Forget (run immediately)
BackgroundJob.Enqueue(() => DoWork());

// 2. Delayed (run after delay or at specific time)
BackgroundJob.Schedule(() => DoWork(), TimeSpan.FromMinutes(10));
BackgroundJob.Schedule(() => DoWork(), DateTime.UtcNow.AddHours(1));

// 3. Recurring (run on schedule)
RecurringJob.AddOrUpdate("job-id", () => DoWork(), Cron.Daily());

// 4. Continuation (run after another job)
var jobId = BackgroundJob.Enqueue(() => Step1());
BackgroundJob.ContinueJobWith(jobId, () => Step2());
```

### Common Cron Expressions

```csharp
Cron.Minutely()                    // Every minute
Cron.Hourly()                      // Every hour
Cron.Daily(2, 30)                  // Every day at 2:30 AM
Cron.Weekly(DayOfWeek.Monday)      // Every Monday
Cron.Monthly()                      // First day of month
"*/15 * * * *"                     // Every 15 minutes
"0 */4 * * *"                      // Every 4 hours
```

Use https://crontab.guru to generate cron expressions.

### Error Handling

```csharp
// Automatic retry (default: 3 attempts)
[AutomaticRetry(Attempts = 3)]
public void ProcessPayment() { }

// No retry
[AutomaticRetry(Attempts = 0)]
public void SendEmail() { }

// Custom retry behavior
[AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public void ImportData() { }
```

---

## 🔧 Configuration

### Using InMemory Storage (Development)

Current configuration uses in-memory storage - **no database needed!**

```csharp
builder.Services.AddHangfire(config => config
    .UseInMemoryStorage());
```

**Pros**: Easy setup, no dependencies
**Cons**: Jobs lost on restart

### Using PostgreSQL Storage (Production)

Uncomment in `Program.cs`:

```csharp
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(
            builder.Configuration.GetConnectionString("HangfireConnection"))));
```

Update connection string in `appsettings.json`.

**Pros**: Persistent, survives restarts
**Cons**: Requires database setup

---

## 🎓 Learning Resources

- **Hangfire Documentation**: https://docs.hangfire.io
- **Cron Expression Editor**: https://crontab.guru
- **Gameball Examples**: See production examples in your codebase

---

## 🐛 Troubleshooting

### Jobs not executing
- Check if Hangfire server is running
- Look for errors in console logs
- Check dashboard for failed jobs

### Dashboard shows 401 error
- Current config allows all access (demo only!)
- For production, implement proper authentication

### Jobs disappearing after restart
- You're using InMemory storage
- Switch to PostgreSQL for persistence

---

## ✅ Exercise Solutions

Solutions are included as comments in `ExercisesController.cs`.

To reveal solutions:
1. Find the method for each exercise
2. Look for `// SOLUTION (uncomment):`
3. Uncomment the code block

---

## 🎯 Next Steps

After completing the exercises:

1. ✅ Review production examples in Gameball codebase
2. ✅ Try implementing a background job in a real feature
3. ✅ Experiment with different cron expressions
4. ✅ Add custom filters for logging
5. ✅ Implement proper error handling

---

## 📝 Notes

- This is a **demo project** - dashboard security is disabled
- In production, always:
  - Use persistent storage (PostgreSQL/SQL Server)
  - Secure the dashboard with authentication
  - Add proper error handling and logging
  - Monitor job performance
  - Set appropriate retry policies

---

**Happy Learning! 🎉**
