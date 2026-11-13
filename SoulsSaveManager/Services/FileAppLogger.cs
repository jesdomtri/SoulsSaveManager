using System.Text;

namespace SoulsSaveManager.Services;

public sealed class FileAppLogger : IAppLogger
{
    private readonly string _logPath;
    private readonly object _lock = new();

    public FileAppLogger(string? logPath = null)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SoulsSaveManager", "logs");
        Directory.CreateDirectory(dir);
        _logPath = logPath ?? Path.Combine(dir, "app.log");
    }

    public void Info(string message) => Write("INFO", message, null);

    public void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private void Write(string level, string message, Exception? exception)
    {
        try
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")).Append("] ")
              .Append(level).Append(' ').Append(message);
            if (exception != null)
            {
                sb.AppendLine().Append(exception);
            }
            sb.AppendLine();
            lock (_lock)
            {
                File.AppendAllText(_logPath, sb.ToString());
            }
        }
        catch
        {
            // logging must not break app flow
        }
    }
}
