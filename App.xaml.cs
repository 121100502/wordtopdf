using System.Windows;
using Syncfusion.Licensing;

namespace WordToPdfApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Syncfusion 社区免费授权密钥（请到官网申请后填入）
        // 申请地址：https://www.syncfusion.com/products/communitylicense
        // 未填 Key 时转换功能将抛出许可证异常，供开发调试使用
        SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JAaF1cX2hIfkx3TXxbf1x2ZFxMYlxbRXZPMyBoS35RcEVqWHdeeXddRGBfWEN0VEFZ");

        base.OnStartup(e);
    }
}
