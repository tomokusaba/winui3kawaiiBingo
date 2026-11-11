using System.Runtime.InteropServices;
using KawaiiBingo.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace KawaiiBingo;

/// <summary>Generic Host と WinUI の起動を管理します。</summary>
internal static partial class Program
{
    /// <summary>アプリケーションを開始します。</summary>
    /// <param name="args">起動時の引数です。</param>
    [STAThread]
    private static void Main(string[] args)
    {
        XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();

        using var host = AppHostBuilder.Build(args);
        host.Start();
        try
        {
            Application.Start(_ =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                host.Services.GetRequiredService<App>();
            });
        }
        finally
        {
            host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        }
    }

    /// <summary>WinUI のプロセス要件を確認します。</summary>
    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();
}
