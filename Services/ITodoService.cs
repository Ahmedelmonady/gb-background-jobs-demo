namespace HangfireDemo.Services;

public interface ITodoService
{
    void SendReminder(int todoId, string title);
}

public class TodoService : ITodoService
{
    private readonly ILogger<TodoService> _logger;

    public TodoService(ILogger<TodoService> logger)
    {
        _logger = logger;
    }

    public void SendReminder(int todoId, string title)
    {
        _logger.LogInformation("⏰ REMINDER: Todo #{TodoId} - {Title}", todoId, title);
        _logger.LogInformation("Don't forget to complete your todo!");
    }
}
