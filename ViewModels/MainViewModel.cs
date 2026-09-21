using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WordToPdfApp.Models;
using WordToPdfApp.Services;

namespace WordToPdfApp.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly LogService _logger;
    private readonly WordToPdfService _converter;
    private CancellationTokenSource? _cts;

    public MainViewModel(LogService logger, WordToPdfService converter)
    {
        _logger = logger;
        _converter = converter;
        Settings = new ConvertSettings();
        _logger.Info("轻文 · Word 批量转 PDF 工具已启动");
        _logger.Info("纯本地离线转换，文件不会上传到任何服务器");
    }

    #region 属性

    [ObservableProperty] private ConvertSettings _settings;

    [ObservableProperty] private ObservableCollection<FileItem> _fileList = new();

    [ObservableProperty] private bool _isConverting;

    [ObservableProperty] private bool _isDragOver;

    [ObservableProperty] private double _totalProgress;

    [ObservableProperty] private int _totalCount;

    [ObservableProperty] private int _successCount;

    [ObservableProperty] private int _failedCount;

    public ObservableCollection<string> Logs => _logger.Logs;

    #endregion

    #region 命令 - 选择文件/文件夹

    /// <summary>
    /// 选择文件命令
    /// </summary>
    [RelayCommand]
    private void SelectFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Word 文档|*.doc;*.docx;*.rtf|所有文件|*.*",
            Multiselect = true,
            Title = "选择 Word 文件"
        };

        if (dialog.ShowDialog() == true)
        {
            AddFiles(dialog.FileNames);
        }
    }

    /// <summary>
    /// 选择文件夹命令
    /// </summary>
    [RelayCommand]
    private void SelectFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择包含 Word 文件的文件夹"
        };

        if (dialog.ShowDialog() == true)
        {
            ScanFolder(dialog.FolderName);
        }
    }

    /// <summary>
    /// 选择输出目录
    /// </summary>
    [RelayCommand]
    private void SelectOutputFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择输出目录"
        };

        if (dialog.ShowDialog() == true)
        {
            Settings.OutputDirectory = dialog.FolderName;
            if (!string.IsNullOrEmpty(Settings.OutputDirectory))
            {
                Settings.OutputToSource = false;
            }
        }
    }

    #endregion

    #region 命令 - 文件列表管理

    /// <summary>
    /// 移除单个文件
    /// </summary>
    [RelayCommand]
    private void RemoveFile(FileItem? item)
    {
        if (item == null) return;
        FileList.Remove(item);
        UpdateStats();
    }

    /// <summary>
    /// 清空列表
    /// </summary>
    [RelayCommand]
    private void ClearList()
    {
        if (FileList.Count == 0) return;
        FileList.Clear();
        UpdateStats();
        _logger.Info("已清空文件列表");
    }

    /// <summary>
    /// 重试失败项
    /// </summary>
    [RelayCommand]
    private void RetryFailed()
    {
        var retry = FileList.Where(x => x.Status == ConvertStatus.Failed).ToList();
        if (retry.Count == 0)
        {
            _logger.Info("没有失败的文件需要重试");
            return;
        }
        foreach (var item in retry)
        {
            item.Status = ConvertStatus.Pending;
            item.Progress = 0;
            item.Message = string.Empty;
        }
        _logger.Info($"已重置 {retry.Count} 个失败文件为待转换状态");
        UpdateStats();
    }

    #endregion

    #region 命令 - 开始转换 / 取消

    /// <summary>
    /// 开始批量转换
    /// </summary>
    [RelayCommand]
    private async Task StartConvert()
    {
        if (IsConverting) return;

        var pendingFiles = FileList.Where(x => x.Status == ConvertStatus.Pending).ToList();
        if (pendingFiles.Count == 0)
        {
            _logger.Warning("没有待转换的文件");
            return;
        }

        // 输出目录校验
        if (!Settings.OutputToSource && string.IsNullOrEmpty(Settings.OutputDirectory))
        {
            _logger.Error("请先设置输出目录");
            return;
        }

        IsConverting = true;
        _cts = new CancellationTokenSource();

        try
        {
            _logger.Info($"开始批量转换，共 {pendingFiles.Count} 个文件");
            TotalProgress = 0;
            int processed = 0;

            foreach (var item in pendingFiles)
            {
                if (_cts.IsCancellationRequested) break;

                var progress = new Progress<double>(p =>
                {
                    item.Progress = p;
                    double overall = (processed + p / 100.0) / pendingFiles.Count * 100;
                    TotalProgress = Math.Min(99.9, overall);
                });

                await _converter.ConvertAsync(item, Settings, progress, _cts.Token);

                processed++;
                UpdateStats();
                TotalProgress = (double)processed / pendingFiles.Count * 100;
            }

            if (_cts.IsCancellationRequested)
            {
                _logger.Warning("转换已取消");
            }
            else
            {
                _logger.Success($"批量转换完成：成功 {SuccessCount}，失败 {FailedCount}");
                // 打开输出目录
                if (Settings.OpenFolderWhenDone && SuccessCount > 0)
                {
                    OpenOutputFolder();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"批量转换异常：{ex.Message}");
        }
        finally
        {
            IsConverting = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// 取消转换
    /// </summary>
    [RelayCommand]
    private void CancelConvert()
    {
        _cts?.Cancel();
        _logger.Info("正在取消...");
    }

    #endregion

    #region 拖拽支持

    /// <summary>
    /// 从拖拽路径列表中添加文件（供 View 调用）
    /// </summary>
    public void AddFromDragDrop(string[] paths)
    {
        var files = new List<string>();
        var folders = new List<string>();

        foreach (var path in paths)
        {
            if (Directory.Exists(path))
                folders.Add(path);
            else if (File.Exists(path))
                files.Add(path);
        }

        if (files.Count > 0) AddFiles(files.ToArray());
        foreach (var folder in folders) ScanFolder(folder);
    }

    #endregion

    #region 私有方法

    private void AddFiles(string[] filePaths)
    {
        int added = 0;
        foreach (var path in filePaths)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (!IsSupportedFormat(ext)) continue;

            // 去重
            if (FileList.Any(x => string.Equals(x.FilePath, path, StringComparison.OrdinalIgnoreCase)))
                continue;

            var fileInfo = new FileInfo(path);
            var item = new FileItem
            {
                FileName = fileInfo.Name,
                FilePath = path,
                Format = ext.TrimStart('.'),
                FileSize = fileInfo.Length,
                Status = ConvertStatus.Pending
            };
            FileList.Add(item);
            added++;
        }
        if (added > 0)
        {
            _logger.Info($"已添加 {added} 个 Word 文件");
        }
        else
        {
            _logger.Info("没有找到支持的 Word 文件");
        }
        UpdateStats();
    }

    private void ScanFolder(string folderPath)
    {
        try
        {
            var searchOption = Settings.RecursiveSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(folderPath, "*.*", searchOption)
                .Where(f => IsSupportedFormat(Path.GetExtension(f).ToLowerInvariant()))
                .ToArray();

            if (files.Length == 0)
            {
                _logger.Info($"文件夹中未找到 Word 文件：{folderPath}");
                return;
            }

            AddFiles(files);
        }
        catch (Exception ex)
        {
            _logger.Error($"扫描文件夹失败：{ex.Message}");
        }
    }

    private static bool IsSupportedFormat(string ext)
    {
        return ext == ".doc" || ext == ".docx" || ext == ".rtf";
    }

    private void UpdateStats()
    {
        TotalCount = FileList.Count;
        SuccessCount = FileList.Count(x => x.Status == ConvertStatus.Success || x.Status == ConvertStatus.Skipped);
        FailedCount = FileList.Count(x => x.Status == ConvertStatus.Failed);
    }

    private void OpenOutputFolder()
    {
        try
        {
            string folder;
            if (!Settings.OutputToSource && !string.IsNullOrEmpty(Settings.OutputDirectory))
            {
                folder = Settings.OutputDirectory;
            }
            else
            {
                // 找到第一个成功文件的输出目录
                var firstSuccess = FileList.FirstOrDefault(x => x.Status == ConvertStatus.Success);
                if (firstSuccess == null) return;
                var sourceDir = Path.GetDirectoryName(firstSuccess.FilePath) ?? string.Empty;
                folder = Path.Combine(sourceDir, "PDF输出");
            }

            if (Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            }
        }
        catch
        {
            // 忽略
        }
    }

    #endregion
}
