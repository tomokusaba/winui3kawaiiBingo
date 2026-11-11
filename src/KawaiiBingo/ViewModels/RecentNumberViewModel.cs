using Microsoft.UI.Xaml.Media;

namespace KawaiiBingo.ViewModels;

/// <summary>最近抽選された番号の表示を表します。</summary>
public sealed class RecentNumberViewModel
{
    /// <summary>最近の番号を作成します。</summary>
    /// <param name="number">抽選された番号です。</param>
    public RecentNumberViewModel(int number)
    {
        Text = $"{BingoPalette.Letter(number)}-{number}";
        Background = BingoPalette.Light(number);
        Foreground = BingoPalette.Ink(number);
    }

    /// <summary>番号とアルファベットの表示を取得します。</summary>
    public string Text { get; }

    /// <summary>番号の背景色を取得します。</summary>
    public Brush Background { get; }

    /// <summary>番号の文字色を取得します。</summary>
    public Brush Foreground { get; }
}
