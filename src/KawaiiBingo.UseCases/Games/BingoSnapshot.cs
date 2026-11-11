using KawaiiBingo.Domain;

namespace KawaiiBingo.UseCases.Games;

/// <summary>画面に渡す、変更されない抽選状態です。</summary>
public sealed class BingoSnapshot
{
    /// <summary>抽選済み番号の検索用集合です。</summary>
    private readonly HashSet<int> _drawn;

    /// <summary>抽選状態のコピーを作成します。</summary>
    /// <param name="game">コピー元の抽選状態です。</param>
    public BingoSnapshot(BingoGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        var numbers = game.DrawnNumbers.ToArray();
        DrawnNumbers = Array.AsReadOnly(numbers);
        RemainingNumbers = Array.AsReadOnly(game.GetRemainingNumbers().ToArray());
        _drawn = [.. numbers];
    }

    /// <summary>抽選済みの番号を抽選順に取得します。</summary>
    public IReadOnlyList<int> DrawnNumbers { get; }

    /// <summary>まだ抽選されていない番号を取得します。</summary>
    public IReadOnlyList<int> RemainingNumbers { get; }

    /// <summary>抽選済みの番号の個数を取得します。</summary>
    public int DrawnCount => DrawnNumbers.Count;

    /// <summary>残りの番号の個数を取得します。</summary>
    public int RemainingCount => RemainingNumbers.Count;

    /// <summary>すべての番号を抽選したかどうかを取得します。</summary>
    public bool IsComplete => RemainingCount == 0;

    /// <summary>最後に抽選された番号を取得します。</summary>
    public int? LastNumber => DrawnCount == 0 ? null : DrawnNumbers[^1];

    /// <summary>指定した番号が抽選済みかどうかを返します。</summary>
    /// <param name="number">確認する番号です。</param>
    /// <returns>抽選済みの場合は真です。</returns>
    public bool IsDrawn(int number) => _drawn.Contains(number);
}
