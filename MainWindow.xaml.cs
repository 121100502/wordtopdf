using System.Windows;
using System.Windows.Input;
using WordToPdfApp.ViewModels;

namespace WordToPdfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            // 延迟显示 Growl，避免初始化时空白气泡闪烁
            WindowGrowl.Visibility = Visibility.Visible;
        };
    }

    private MainViewModel VM => (MainViewModel)DataContext;

    private void DragZone_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            VM.IsDragOver = true;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void DragZone_DragLeave(object sender, DragEventArgs e)
    {
        VM.IsDragOver = false;
    }

    private void DragZone_Drop(object sender, DragEventArgs e)
    {
        VM.IsDragOver = false;
        if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] paths)
        {
            VM.AddFromDragDrop(paths);
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OutputDir_Click(object sender, MouseButtonEventArgs e)
    {
        VM.SelectOutputFolderCommand.Execute(null);
    }
}
