public class FileLogger : ILogger
{
  private readonly string _filePath;

  private readonly object _lock = new();

  public FileLogger(string filePath)
  {
    if (filePath == null) throw new ArgumentNullException(nameof(filePath));
    
    _filePath = filePath;
  }

  IDisposable ILogger.BeginScope<TState>(TState state) => null!;

  public bool IsEnabled(LogLevel logLevel)
  {
    return logLevel >= LogLevel.Warning;
  }

  public void Log<TState>(LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
    if (!IsEnabled(logLevel))
    {
      return;
    }

    lock (_lock)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
      using (var writer = new StreamWriter(_filePath, true))
      {
        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}");


        if (exception != null)
        {
          writer.WriteLine("Exception: " + exception.Message);
          writer.WriteLine(exception.StackTrace);
          if (exception.InnerException != null)
          {
            writer.WriteLine("Inner Exception: " + exception.InnerException.Message);
            writer.WriteLine(exception.InnerException.StackTrace);
          }

        }
        writer.WriteLine(new string('-', 80));
      }
    }
  }
}