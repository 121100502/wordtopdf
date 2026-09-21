using CommunityToolkit.Mvvm.ComponentModel;

namespace WordToPdfApp.Models;

/// <summary>
/// 转换任务状态
/// </summary>
public enum ConvertStatus
{
    /// <summary>待转换</summary>
    Pending,

    /// <summary>转换中</summary>
    Converting,

    /// <summary>成功</summary>
    Success,

    /// <summary>失败</summary>
    Failed,

    /// <summary>已跳过</summary>
    Skipped
}

/// <summary>
/// 文件转换项
/// </summary>
public partial class FileItem : ObservableObject
{
    [ObservableProperty] private string _fileName = string.Empty;

    [ObservableProperty] private string _filePath = string.Empty;

    [ObservableProperty] private string _format = string.Empty;

    [ObservableProperty] private long _fileSize;

    [ObservableProperty] private ConvertStatus _status = ConvertStatus.Pending;

    [ObservableProperty] private string _message = string.Empty;

    [ObservableProperty] private double _progress;

    /// <summary>文件大小展示文本</summary>
    public string FileSizeText => FormatFileSize(FileSize);

    /// <summary>状态显示文本</summary>
    public string StatusText => Status switch
    {
        ConvertStatus.Pending => "待转换",
        ConvertStatus.Converting => "转换中",
        ConvertStatus.Success => "成功",
        ConvertStatus.Failed => "失败",
        ConvertStatus.Skipped => "已跳过",
        _ => string.Empty
    };

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:0.0} KB";
        return $"{bytes / 1024.0 / 1024.0:0.0} MB";
    }
}
