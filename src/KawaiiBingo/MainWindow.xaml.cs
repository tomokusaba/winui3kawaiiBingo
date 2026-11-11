using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace KawaiiBingo;

/// <summary>ビンゴ画面を表示するメインウィンドウです。</summary>
public sealed partial class MainWindow : Window
{
    /// <summary>メイン画面をウィンドウに配置します。</summary>
    /// <param name="page">表示するメイン画面です。</param>
    public MainWindow(MainPage page)
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        PageHost.Content = page;
        ConfigureWindow();
    }

    /// <summary>画面に収まる大きさと位置を設定します。</summary>
    private void ConfigureWindow()
    {
        AppWindow.Title = "かわいいビンゴ";
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(iconPath))
        {
            AppWindow.SetIcon(iconPath);
        }

        var handle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var scale = GetDpiForWindow(handle) / 96.0;
        var display = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        if (display is not null)
        {
            var area = display.WorkArea;
            var width = Math.Min((int)(1420 * scale), (int)(area.Width * 0.94));
            var height = Math.Min((int)(900 * scale), (int)(area.Height * 0.94));
            AppWindow.MoveAndResize(new RectInt32(
                area.X + (area.Width - width) / 2,
                area.Y + (area.Height - height) / 2,
                width,
                height));
        }

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(650 * scale);
            presenter.PreferredMinimumHeight = (int)(480 * scale);
        }
    }

    /// <summary>ウィンドウが表示されるディスプレイの DPI を取得します。</summary>
    /// <param name="window">対象ウィンドウのハンドルです。</param>
    /// <returns>ウィンドウの DPI です。</returns>
    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint window);
}
