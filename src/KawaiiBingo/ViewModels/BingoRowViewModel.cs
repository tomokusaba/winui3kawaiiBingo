using Microsoft.UI.Xaml.Media;

namespace KawaiiBingo.ViewModels;

/// <summary>ビンゴ盤のアルファベット一行を表します。</summary>
public sealed class BingoRowViewModel
{
    /// <summary>行の番号を作成します。</summary>
    /// <param name="firstNumber">行の最初の番号です。</param>
    public BingoRowViewModel(int firstNumber)
    {
        Letter = BingoPalette.Letter(firstNumber);
        Background = BingoPalette.Light(firstNumber);
        Foreground = BingoPalette.Ink(firstNumber);
        Cells = Enumerable.Range(firstNumber, 15).Select(number => new BingoCellViewModel(number)).ToArray();
    }

    /// <summary>行に対応するアルファベットを取得します。</summary>
    public string Letter { get; }

    /// <summary>行見出しの背景色を取得します。</summary>
    public Brush Background { get; }

    /// <summary>行見出しの文字色を取得します。</summary>
    public Brush Foreground { get; }

    /// <summary>行に含まれる番号を取得します。</summary>
    public IReadOnlyList<BingoCellViewModel> Cells { get; }
}
