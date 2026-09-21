using WordToPdfApp.Services;
using WordToPdfApp.ViewModels;

namespace WordToPdfApp.Helpers;

/// <summary>
/// 简易的 ViewModel 定位器，用于 XAML 绑定
/// </summary>
public class ViewModelLocator
{
    private static MainViewModel? _main;
    public static MainViewModel Main => _main ??= new MainViewModel(
        new LogService(),
        new WordToPdfService(new LogService()));

    // 重新初始化（让日志服务单例共享）
    static ViewModelLocator()
    {
        var logger = new LogService();
        _main = new MainViewModel(logger, new WordToPdfService(logger));
    }
}
