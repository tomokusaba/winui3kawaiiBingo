using KawaiiBingo.Domain;
using KawaiiBingo.UseCases.Games;

namespace KawaiiBingo.Tests;

/// <summary>ビンゴ抽選の規則を検証します。</summary>
[TestClass]
public sealed class BingoGameTests
{
    /// <summary>すべての番号が重複なく抽選され、最後に終了することを確認します。</summary>
    [TestMethod]
    public void DrawsEveryNumberOnce()
    {
        var game = new BingoGame();
        var random = new FixedRandom(0);

        for (var number = 1; number <= BingoGame.MaximumNumber; number++)
        {
            Assert.AreEqual(number, game.Draw(random));
        }

        Assert.AreEqual(75, game.DrawnNumbers.Distinct().Count());
        Assert.AreEqual(0, game.RemainingCount);
        Assert.IsTrue(game.IsComplete);
        Assert.ThrowsExactly<InvalidOperationException>(() => game.Draw(random));
    }

    /// <summary>範囲外の乱数では抽選状態が変更されないことを確認します。</summary>
    [TestMethod]
    public void InvalidRandomDoesNotMutateGame()
    {
        var game = new BingoGame();

        Assert.ThrowsExactly<InvalidOperationException>(() => game.Draw(new FixedRandom(75)));
        Assert.ThrowsExactly<InvalidOperationException>(() => game.Draw(new FixedRandom(-1)));
        Assert.AreEqual(0, game.DrawnCount);
        Assert.AreEqual(75, game.RemainingCount);
    }

    /// <summary>取り消し後の番号を再び抽選でき、リセットで履歴が消えることを確認します。</summary>
    [TestMethod]
    public void UndoAndResetRestoreAvailableNumbers()
    {
        var game = BingoGame.Restore([1, 25, 75]);

        Assert.AreEqual(75, game.UndoLastDraw());
        Assert.IsFalse(game.IsDrawn(75));
        Assert.AreEqual(75, game.Draw(new FixedRandom(72)));
        game.Reset();
        Assert.AreEqual(0, game.DrawnCount);
        Assert.IsNull(game.UndoLastDraw());
        Assert.AreEqual(75, game.RemainingCount);
    }

    /// <summary>重複または範囲外の保存履歴が拒否されることを確認します。</summary>
    [TestMethod]
    public void InvalidHistoryIsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() => BingoGame.Restore([1, 1]));
        Assert.ThrowsExactly<ArgumentException>(() => BingoGame.Restore([0]));
        Assert.ThrowsExactly<ArgumentException>(() => BingoGame.Restore([76]));
    }

    /// <summary>画面用の状態が元の抽選状態の変更に影響されないことを確認します。</summary>
    [TestMethod]
    public void SnapshotIsIndependentOfGame()
    {
        var game = BingoGame.Restore([5, 23]);
        var snapshot = new BingoSnapshot(game);
        game.UndoLastDraw();
        game.Draw(new FixedRandom(0));

        CollectionAssert.AreEqual(new[] { 5, 23 }, snapshot.DrawnNumbers.ToArray());
        Assert.IsTrue(snapshot.IsDrawn(23));
        Assert.AreEqual(73, snapshot.RemainingCount);
    }

    /// <summary>指定した値を返すテスト用の乱数源です。</summary>
    private sealed class FixedRandom : IRandomSource
    {
        /// <summary>返す値を保持します。</summary>
        private readonly int _value;

        /// <summary>返す値を指定します。</summary>
        /// <param name="value">返す整数です。</param>
        public FixedRandom(int value) => _value = value;

        /// <summary>指定された固定値を返します。</summary>
        /// <param name="exclusiveMaximum">抽選範囲の上限です。</param>
        /// <returns>設定された固定値です。</returns>
        public int Next(int exclusiveMaximum) => _value;
    }
}
