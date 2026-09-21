using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WordToPdfApp.Services;

/// <summary>
/// 日志服务 - 实时输出日志到 UI 并保存到本地文件
/// </summary>
public partial class LogService : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _logs = new();

    private readonly object _lockObj = new();
    private readonly string _logDir;
    private string _logFilePath = string.Empty;

    public LogService()
    {
        _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        if (!Directory.Exists(_logDir)) Directory.CreateDirectory(_logDir);
    }

    public void Info(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [信息] {message}";
        AppendLog(line);
    }

    public void Success(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [成功] {message}";
        AppendLog(line);
    }

    public void Warning(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [警告] {message}";
        AppendLog(line);
    }

    public void Error(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [错误] {message}";
        AppendLog(line);
    }

    public void Clear()
    {
        lock (_lockObj) { Logs.Clear(); }
    }

    private void AppendLog(string line)
    {
        lock (_lockObj)
        {
            // UI 日志只保留最新 200 条，防止内存膨胀
            if (Logs.Count >= 200) Logs.RemoveAt(0);
            Logs.Add(line);
            // 写入文件
            WriteToFile(line);
        }
    }

    private void WriteToFile(string line)
    {
        try
        {
            if (string.IsNullOrEmpty(_logFilePath))
            {
                var fileName = $"convert_{DateTime.Now:yyyyMMdd}.log";
                _logFilePath = Path.Combine(_logDir, fileName);
            }
            File.AppendAllText(_logFilePath, line + Environment.NewLine);
        }
        catch
        {
            // 日志写入失败不影响主流程
        }
    }
}
