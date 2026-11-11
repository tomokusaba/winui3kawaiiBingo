using KawaiiBingo.Domain;
using KawaiiBingo.UseCases.Abstractions;
using KawaiiBingo.UseCases.Games;

namespace KawaiiBingo.Tests;

/// <summary>抽選操作と保存の連携を検証します。</summary>
[TestClass]
public sealed class BingoGameServiceTests
{
    /// <summary>同時に抽選しても履歴が失われず重複しないことを確認します。</summary>
    /// <returns>検証の完了を表すタスクです。</returns>
    [TestMethod]
    public async Task ConcurrentDrawsAreSerialized()
    {
        var repository = new MemoryRepository();
        using var service = new BingoGameService(repository, new FirstRandom());

        await Task.WhenAll(Enumerable.Range(0, 75).Select(_ => service.DrawAsync()));
        var snapshot = await service.GetAsync();

        Assert.AreEqual(75, snapshot.DrawnCount);
        Assert.AreEqual(75, snapshot.DrawnNumbers.Distinct().Count());
        Assert.IsTrue(snapshot.IsComplete);
        await service.DrawAsync();
        Assert.AreEqual(75, repository.SaveCount);
    }

    /// <summary>保存に失敗した抽選が次回の状態に反映されないことを確認します。</summary>
    /// <returns>検証の完了を表すタスクです。</returns>
    [TestMethod]
    public async Task FailedSaveDoesNotChangeStoredHistory()
    {
        var repository = new MemoryRepository { FailWrites = true };
        using var service = new BingoGameService(repository, new FirstRandom());

        await Assert.ThrowsExactlyAsync<IOException>(() => service.DrawAsync());
        Assert.AreEqual(0, (await service.GetAsync()).DrawnCount);
    }

    /// <summary>抽選履歴を独立したコピーとして保持するテスト用リポジトリです。</summary>
    private sealed class MemoryRepository : IBingoGameRepository
    {
        /// <summary>保存された番号です。</summary>
        private int[] _numbers = [];

        /// <summary>保存を失敗させるかどうかを取得または設定します。</summary>
        public bool FailWrites { get; set; }

        /// <summary>保存の成功回数を取得します。</summary>
        public int SaveCount { get; private set; }

        /// <summary>保存された番号から状態を復元します。</summary>
        /// <param name="cancellationToken">処理のキャンセルを通知します。</param>
        /// <returns>復元された抽選状態です。</returns>
        public Task<BingoGame> LoadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(BingoGame.Restore(_numbers));
        }

        /// <summary>抽選状態のコピーを保存します。</summary>
        /// <param name="game">保存する抽選状態です。</param>
        /// <param name="cancellationToken">処理のキャンセルを通知します。</param>
        /// <returns>保存の完了を表すタスクです。</returns>
        public async Task SaveAsync(BingoGame game, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            if (FailWrites)
            {
                throw new IOException("テスト用の保存失敗です。");
            }

            _numbers = game.DrawnNumbers.ToArray();
            SaveCount++;
        }
    }

    /// <summary>常に最初の候補を選ぶテスト用の乱数源です。</summary>
    private sealed class FirstRandom : IRandomSource
    {
        /// <summary>最初の候補を選びます。</summary>
        /// <param name="exclusiveMaximum">抽選範囲の上限です。</param>
        /// <returns>最初の候補の位置です。</returns>
        public int Next(int exclusiveMaximum) => 0;
    }
}
