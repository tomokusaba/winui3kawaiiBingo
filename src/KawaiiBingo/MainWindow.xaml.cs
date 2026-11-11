using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;

namespace KawaiiBingo;

/// <summary>ビンゴ画面を表示するメインウィンドウです。</summary>
public sealed partial class MainWindow : Window
{
    /// <summary>ウィンドウに表示しているビンゴ画面です。</summary>
    private readonly MainPage _page;

    /// <summary>メイン画面をウィンドウに配置します。</summary>
    /// <param name="page">表示するメイン画面です。</param>
    public MainWindow(MainPage page)
    {
        _page = page;
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        PageHost.Content = page;
        page.FullScreenRequested += OnFullScreenRequested;
        AppWindow.Changed += OnAppWindowChanged;
        ConfigureWindow();
    }

    /// <summary>画面の全画面ボタンから表示を切り替えます。</summary>
    /// <param name="sender">ビンゴ画面です。</param>
    /// <param name="args">要求の情報です。</param>
    private void OnFullScreenRequested(object? sender, EventArgs args) => ToggleFullScreen();

    /// <summary>F11 キーから全画面表示を切り替えます。</summary>
    /// <param name="sender">キーボード操作です。</param>
    /// <param name="args">操作の情報です。</param>
    private void OnFullScreenAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ToggleFullScreen();
        args.Handled = true;
    }

    /// <summary>全画面表示中だけ Esc キーで元のウィンドウに戻します。</summary>
    /// <param name="sender">キーボード操作です。</param>
    /// <param name="args">操作の情報です。</param>
    private void OnExitFullScreenAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            args.Handled = true;
        }
    }

    /// <summary>現在のウィンドウ位置を維持して、全画面表示を切り替えます。</summary>
    private void ToggleFullScreen() => AppWindow.SetPresenter(
        AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen
            ? AppWindowPresenterKind.Overlapped
            : AppWindowPresenterKind.FullScreen);

    /// <summary>全画面ではタイトルバーの余白をなくし、終了の案内を更新します。</summary>
    /// <param name="sender">変更されたウィンドウです。</param>
    /// <param name="args">変更の情報です。</param>
    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidPresenterChange)
        {
            var isFullScreen = sender.Presenter.Kind == AppWindowPresenterKind.FullScreen;
            TitleBarRow.Height = new GridLength(isFullScreen ? 0 : 40);
            AppTitleBar.Visibility = isFullScreen ? Visibility.Collapsed : Visibility.Visible;
            _page.SetFullScreenState(isFullScreen);
        }
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
