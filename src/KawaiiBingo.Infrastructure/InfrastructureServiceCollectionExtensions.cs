using KawaiiBingo.Domain;
using KawaiiBingo.Infrastructure.Persistence;
using KawaiiBingo.Infrastructure.Randomness;
using KawaiiBingo.UseCases.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KawaiiBingo.Infrastructure;

/// <summary>保存と乱数の実装を依存関係コンテナーに登録します。</summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>インフラストラクチャーのサービスを登録します。</summary>
    /// <param name="services">登録先のサービスコレクションです。</param>
    /// <param name="configuration">保存先の設定です。</param>
    /// <returns>登録先のサービスコレクションです。</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var configuredDirectory = configuration["Storage:DataDirectory"];
        var dataDirectory = string.IsNullOrWhiteSpace(configuredDirectory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KawaiiBingo")
            : Path.GetFullPath(Environment.ExpandEnvironmentVariables(configuredDirectory));

        services.AddSingleton<IRandomSource, CryptoRandomSource>();
        services.AddSingleton<IBingoGameRepository>(provider => new JsonBingoGameRepository(
            dataDirectory, provider.GetRequiredService<ILogger<JsonBingoGameRepository>>()));
        return services;
    }
}
