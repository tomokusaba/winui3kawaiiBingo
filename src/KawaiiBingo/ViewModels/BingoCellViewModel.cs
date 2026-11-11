using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace KawaiiBingo.ViewModels;

/// <summary>ビンゴ盤の一つの番号と表示状態を表します。</summary>
public sealed class BingoCellViewModel : ObservableObject
{
    /// <summary>番号が抽選済みかどうかです。</summary>
    private bool _isDrawn;

    /// <summary>番号が最新の抽選結果かどうかです。</summary>
    private bool _isLatest;

    /// <summary>表示する番号を指定します。</summary>
    /// <param name="number">ビンゴの番号です。</param>
    public BingoCellViewModel(int number) => Number = number;

    /// <summary>ビンゴの番号を取得します。</summary>
    public int Number { get; }

    /// <summary>表示用の番号を取得します。</summary>
    public string NumberText => Number.ToString(CultureInfo.InvariantCulture);

    /// <summary>セルの背景色を取得します。</summary>
    public Brush Background => _isDrawn ? BingoPalette.Light(Number) : BingoPalette.EmptyBackground;

    /// <summary>セルの文字色を取得します。</summary>
    public Brush Foreground => _isDrawn ? BingoPalette.Ink(Number) : BingoPalette.EmptyInk;

    /// <summary>セルの枠色を取得します。</summary>
    public Brush BorderBrush => _isLatest ? BingoPalette.LatestBorder : BingoPalette.EmptyBorder;

    /// <summary>支援技術向けに番号と抽選状態を取得します。</summary>
    public string AutomationName => _isLatest
        ? $"{BingoPalette.Letter(Number)} の {Number}、最新の抽選番号"
        : $"{BingoPalette.Letter(Number)} の {Number}、{(_isDrawn ? "抽選済み" : "未抽選")}";

    /// <summary>抽選状態に合わせて表示を更新します。</summary>
    /// <param name="isDrawn">抽選済みの場合は真です。</param>
    /// <param name="isLatest">最新の番号の場合は真です。</param>
    public void SetState(bool isDrawn, bool isLatest)
    {
        if (_isDrawn == isDrawn && _isLatest == isLatest)
        {
            return;
        }

        _isDrawn = isDrawn;
        _isLatest = isLatest;
        OnPropertyChanged(nameof(Background));
        OnPropertyChanged(nameof(Foreground));
        OnPropertyChanged(nameof(BorderBrush));
        OnPropertyChanged(nameof(AutomationName));
    }
}
