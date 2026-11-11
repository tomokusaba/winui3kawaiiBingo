using KawaiiBingo.Infrastructure;
using KawaiiBingo.UseCases;
using KawaiiBingo.UseCases.Games;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KawaiiBingo.Tests;

/// <summary>抽選履歴の保存と読み込みを検証します。</summary>
[TestClass]
public sealed class PersistenceTests
{
    /// <summary>テスト専用の保存先です。</summary>
    private string _directory = null!;

    /// <summary>テストごとに保存先を作成します。</summary>
    [TestInitialize]
    public void CreateDirectory()
    {
        _directory = Path.Combine(Path.GetTempPath(), "KawaiiBingo.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    /// <summary>テスト専用の保存先を削除します。</summary>
    [TestCleanup]
    public void DeleteDirectory() => Directory.Delete(_directory, recursive: true);

    /// <summary>抽選、再読み込み、取り消し、リセットが保存されることを確認します。</summary>
    /// <returns>検証の完了を表すタスクです。</returns>
    [TestMethod]
    public async Task HistorySurvivesRestartAndCanBeReset()
    {
        using (var provider = CreateProvider())
        {
            var service = provider.GetRequiredService<BingoGameService>();
            Assert.AreEqual(0, (await service.GetAsync()).DrawnCount);
            await service.DrawAsync();
            await service.DrawAsync();
        }

        using (var provider = CreateProvider())
        {
            var service = provider.GetRequiredService<BingoGameService>();
            var loaded = await service.GetAsync();
            Assert.AreEqual(2, loaded.DrawnCount);
            Assert.AreNotEqual(loaded.DrawnNumbers[0], loaded.DrawnNumbers[1]);
            Assert.AreEqual(1, (await service.UndoAsync()).DrawnCount);
            Assert.AreEqual(0, (await service.ResetAsync()).DrawnCount);
        }

        using var finalProvider = CreateProvider();
        Assert.AreEqual(0, (await finalProvider.GetRequiredService<BingoGameService>().GetAsync()).DrawnCount);
    }

    /// <summary>破損した履歴を初期状態として黙って上書きしないことを確認します。</summary>
    /// <returns>検証の完了を表すタスクです。</returns>
    [TestMethod]
    public async Task CorruptHistoryIsReportedAndPreserved()
    {
        var path = Path.Combine(_directory, "game.json");
        await File.WriteAllTextAsync(path, "{ broken");
        using var provider = CreateProvider();
        var service = provider.GetRequiredService<BingoGameService>();

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() => service.GetAsync());
        await Assert.ThrowsExactlyAsync<InvalidDataException>(() => service.DrawAsync());
        Assert.AreEqual("{ broken", await File.ReadAllTextAsync(path));
    }

    /// <summary>不正な番号を含む履歴が拒否されることを確認します。</summary>
    /// <returns>検証の完了を表すタスクです。</returns>
    [TestMethod]
    public async Task InvalidNumbersAreReported()
    {
        await File.WriteAllTextAsync(
            Path.Combine(_directory, "game.json"), """{"version":1,"drawnNumbers":[3,3]}""");
        using var provider = CreateProvider();

        await Assert.ThrowsExactlyAsync<InvalidDataException>(
            () => provider.GetRequiredService<BingoGameService>().GetAsync());
    }

    /// <summary>テスト専用の依存関係コンテナーを作成します。</summary>
    /// <returns>構築されたサービスプロバイダーです。</returns>
    private ServiceProvider CreateProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:DataDirectory"] = _directory })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddUseCases();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider(validateScopes: true);
    }
}
