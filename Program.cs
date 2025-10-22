using Hangfire;
using Hangfire.Dashboard;
using Hangfire.InMemory;
using HangfireDemo.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICleanupService, CleanupService>();
builder.Services.AddScoped<ITodoService, TodoService>();

// Configure Hangfire with InMemory storage (for demo purposes)
// In production, use PostgreSQL or SQL Server
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());  // Easy for demo - no database needed!

// Alternative: PostgreSQL Storage (commented out)
// builder.Services.AddHangfire(config => config
//     .UsePostgreSqlStorage(options =>
//         options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection"))));

// Add Hangfire server
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.Queues = new[] { "critical", "default" };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() },
    DashboardTitle = "Hangfire Demo Dashboard"
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Log.Information("Application starting...");
Log.Information("Hangfire Dashboard: https://localhost:7000/hangfire");

app.Run();

// Allow all users to access dashboard (only for demo!)
public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
