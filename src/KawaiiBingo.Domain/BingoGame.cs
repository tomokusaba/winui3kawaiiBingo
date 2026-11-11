using System.Collections.ObjectModel;

namespace KawaiiBingo.Domain;

/// <summary>一回のビンゴ抽選の番号と履歴を管理します。</summary>
public sealed class BingoGame
{
    /// <summary>抽選する最小の番号です。</summary>
    public const int MinimumNumber = 1;

    /// <summary>抽選する最大の番号です。</summary>
    public const int MaximumNumber = 75;

    /// <summary>各アルファベットに対応する番号の個数です。</summary>
    public const int NumbersPerLetter = 15;

    /// <summary>抽選済みの番号を順番に保持します。</summary>
    private readonly List<int> _drawnNumbers = [];

    /// <summary>番号が抽選済みかを保持します。</summary>
    private readonly bool[] _drawn = new bool[MaximumNumber + 1];

    /// <summary>履歴の読み取り専用ビューです。</summary>
    private readonly ReadOnlyCollection<int> _drawnNumbersView;

    /// <summary>抽選前の状態を作成します。</summary>
    public BingoGame()
    {
        _drawnNumbersView = _drawnNumbers.AsReadOnly();
    }

    /// <summary>抽選済みの番号を抽選順に取得します。</summary>
    public IReadOnlyList<int> DrawnNumbers => _drawnNumbersView;

    /// <summary>抽選済みの番号の個数を取得します。</summary>
    public int DrawnCount => _drawnNumbers.Count;

    /// <summary>まだ抽選されていない番号の個数を取得します。</summary>
    public int RemainingCount => MaximumNumber - DrawnCount;

    /// <summary>すべての番号を抽選したかどうかを取得します。</summary>
    public bool IsComplete => RemainingCount == 0;

    /// <summary>最後に抽選された番号を取得します。</summary>
    public int? LastNumber => DrawnCount == 0 ? null : _drawnNumbers[^1];

    /// <summary>保存された抽選履歴から状態を復元します。</summary>
    /// <param name="drawnNumbers">抽選順に並んだ番号です。</param>
    /// <returns>復元された抽選状態です。</returns>
    public static BingoGame Restore(IEnumerable<int> drawnNumbers)
    {
        ArgumentNullException.ThrowIfNull(drawnNumbers);

        var game = new BingoGame();
        foreach (var number in drawnNumbers)
        {
            if (number is < MinimumNumber or > MaximumNumber || game._drawn[number])
            {
                throw new ArgumentException("抽選履歴に範囲外または重複した番号があります。", nameof(drawnNumbers));
            }

            game._drawn[number] = true;
            game._drawnNumbers.Add(number);
        }

        return game;
    }

    /// <summary>指定した番号が抽選済みかどうかを返します。</summary>
    /// <param name="number">確認する番号です。</param>
    /// <returns>抽選済みの場合は真です。</returns>
    public bool IsDrawn(int number) =>
        number is >= MinimumNumber and <= MaximumNumber && _drawn[number];

    /// <summary>まだ抽選されていない番号を昇順で取得します。</summary>
    /// <returns>残っている番号の一覧です。</returns>
    public IReadOnlyList<int> GetRemainingNumbers()
    {
        var remaining = new List<int>(RemainingCount);
        for (var number = MinimumNumber; number <= MaximumNumber; number++)
        {
            if (!_drawn[number])
            {
                remaining.Add(number);
            }
        }

        return remaining.AsReadOnly();
    }

    /// <summary>残っている番号から一つを抽選します。</summary>
    /// <param name="random">抽選に使用する乱数の提供元です。</param>
    /// <returns>新しく抽選された番号です。</returns>
    public int Draw(IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (IsComplete)
        {
            throw new InvalidOperationException("すべての番号が抽選済みです。");
        }

        var remaining = GetRemainingNumbers();
        var index = random.Next(remaining.Count);
        if (index < 0 || index >= remaining.Count)
        {
            throw new InvalidOperationException("乱数の値が抽選範囲外です。");
        }

        var number = remaining[index];
        _drawn[number] = true;
        _drawnNumbers.Add(number);
        return number;
    }

    /// <summary>最後に抽選した番号を履歴から戻します。</summary>
    /// <returns>戻した番号、履歴がない場合は null です。</returns>
    public int? UndoLastDraw()
    {
        if (LastNumber is not int number)
        {
            return null;
        }

        _drawnNumbers.RemoveAt(_drawnNumbers.Count - 1);
        _drawn[number] = false;
        return number;
    }

    /// <summary>抽選履歴を消して最初の状態に戻します。</summary>
    public void Reset()
    {
        _drawnNumbers.Clear();
        Array.Clear(_drawn);
    }
}
