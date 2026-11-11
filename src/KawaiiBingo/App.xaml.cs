using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace KawaiiBingo;

/// <summary>ビンゴアプリケーションの起動を管理します。</summary>
public partial class App : Application
{
    /// <summary>起動するウィンドウの依存関係を解決します。</summary>
    private readonly IServiceProvider _services;

    /// <summary>未処理のエラーを記録します。</summary>
    private readonly ILogger<App> _logger;

    /// <summary>表示中のウィンドウを保持します。</summary>
    private Window? _window;

    /// <summary>アプリケーションを初期化します。</summary>
    /// <param name="services">依存関係の提供元です。</param>
    /// <param name="logger">ログの記録先です。</param>
    public App(IServiceProvider services, ILogger<App> logger)
    {
        _services = services;
        _logger = logger;
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    /// <summary>起動時にメインウィンドウを表示します。</summary>
    /// <param name="args">起動時の情報です。</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = _services.GetRequiredService<MainWindow>();
        _window.Activate();
    }

    /// <summary>未処理の例外をログに記録します。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="args">発生した例外の情報です。</param>
    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        _logger.LogCritical(args.Exception, "アプリケーションで予期しないエラーが発生しました。");
    }
}
