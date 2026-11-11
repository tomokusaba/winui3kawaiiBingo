using KawaiiBingo.Infrastructure;
using KawaiiBingo.UseCases;
using KawaiiBingo.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KawaiiBingo.Hosting;

/// <summary>アプリケーションの依存関係を組み立てます。</summary>
internal static class AppHostBuilder
{
    /// <summary>WinUI 用の Generic Host を構築します。</summary>
    /// <param name="args">起動時の引数です。</param>
    /// <returns>構築されたホストです。</returns>
    public static IHost Build(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ApplicationName = typeof(App).Assembly.GetName().Name,
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();
        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        }));

        builder.Services.AddSingleton<IHostLifetime, WinUiHostLifetime>();
        builder.Services.AddUseCases();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainViewModel>();
        return builder.Build();
    }
}
