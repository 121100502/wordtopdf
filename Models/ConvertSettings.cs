using CommunityToolkit.Mvvm.ComponentModel;

namespace WordToPdfApp.Models;

/// <summary>
/// 转换设置
/// </summary>
public partial class ConvertSettings : ObservableObject
{
    /// <summary>输出目录（空则使用原目录）</summary>
    [ObservableProperty] private string _outputDirectory = string.Empty;

    /// <summary>是否输出到原目录</summary>
    [ObservableProperty] private bool _outputToSource = true;

    /// <summary>是否递归扫描子文件夹</summary>
    [ObservableProperty] private bool _recursiveSubfolders = false;

    /// <summary>覆盖策略：true=覆盖，false=跳过</summary>
    [ObservableProperty] private bool _overwriteExisting = false;

    /// <summary>转换完成后自动打开输出文件夹</summary>
    [ObservableProperty] private bool _openFolderWhenDone = true;
}
