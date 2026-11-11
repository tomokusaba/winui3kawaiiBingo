using KawaiiBingo.UseCases.Games;
using Microsoft.Extensions.DependencyInjection;

namespace KawaiiBingo.UseCases;

/// <summary>ユースケースを依存関係コンテナーに登録します。</summary>
public static class UseCasesServiceCollectionExtensions
{
    /// <summary>抽選のユースケースを登録します。</summary>
    /// <param name="services">登録先のサービスコレクションです。</param>
    /// <returns>登録先のサービスコレクションです。</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddSingleton<BingoGameService>();
        return services;
    }
}
