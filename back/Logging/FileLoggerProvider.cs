public class FileLoggerProvider : ILoggerProvider
{
  private readonly string _filePath;

  public FileLoggerProvider(string filePath)
  {
    if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        
    _filePath = filePath;
  }

  public ILogger CreateLogger(string categoryName)
  {
    return new FileLogger(_filePath);
  }

  public void Dispose() { }
}