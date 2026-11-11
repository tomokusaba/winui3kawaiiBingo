using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace KawaiiBingo.ViewModels;

/// <summary>ビンゴの列ごとに使う色を提供します。</summary>
internal static class BingoPalette
{
    /// <summary>ボールの色です。</summary>
    private static readonly SolidColorBrush[] BallColors =
    [
        Brush(0xF5, 0x78, 0xA6), Brush(0xF3, 0x9B, 0x62), Brush(0xEF, 0xC4, 0x43),
        Brush(0x68, 0xCA, 0xA2), Brush(0xA8, 0x8B, 0xEE)
    ];

    /// <summary>列と抽選済みのセルの淡い色です。</summary>
    private static readonly SolidColorBrush[] LightColors =
    [
        Brush(0xFF, 0xD5, 0xE5), Brush(0xFF, 0xE1, 0xC9), Brush(0xFF, 0xEF, 0xB4),
        Brush(0xCB, 0xF0, 0xDF), Brush(0xE7, 0xDE, 0xFF)
    ];

    /// <summary>各列の文字に使う濃い色です。</summary>
    private static readonly SolidColorBrush[] InkColors =
    [
        Brush(0x94, 0x2F, 0x59), Brush(0x95, 0x47, 0x19), Brush(0x76, 0x5B, 0x0C),
        Brush(0x17, 0x70, 0x50), Brush(0x60, 0x42, 0x9D)
    ];

    /// <summary>未抽選のセルの背景色です。</summary>
    public static SolidColorBrush EmptyBackground { get; } = Brush(0xFF, 0xF7, 0xF8);

    /// <summary>未抽選のセルの文字色です。</summary>
    public static SolidColorBrush EmptyInk { get; } = Brush(0x79, 0x67, 0x77);

    /// <summary>通常のセルの枠色です。</summary>
    public static SolidColorBrush EmptyBorder { get; } = Brush(0xF6, 0xE7, 0xEA);

    /// <summary>最新のセルの枠色です。</summary>
    public static SolidColorBrush LatestBorder { get; } = Brush(0xE5, 0x5C, 0x8B);

    /// <summary>抽選演出でスポットライトが当たったセルの背景色です。</summary>
    public static SolidColorBrush SpotlightBackground { get; } = Brush(0xFF, 0xF3, 0xB8);

    /// <summary>抽選演出でスポットライトが当たったセルの枠色です。</summary>
    public static SolidColorBrush SpotlightBorder { get; } = Brush(0xF2, 0xA9, 0x2E);

    /// <summary>番号に対応するアルファベットを返します。</summary>
    /// <param name="number">ビンゴの番号です。</param>
    /// <returns>対応するアルファベットです。</returns>
    public static string Letter(int number) => "BINGO"[Index(number)].ToString();

    /// <summary>番号に対応するボールの色を返します。</summary>
    /// <param name="number">ビンゴの番号です。</param>
    /// <returns>対応するボールのブラシです。</returns>
    public static SolidColorBrush Ball(int number) => BallColors[Index(number)];

    /// <summary>番号に対応する淡い色を返します。</summary>
    /// <param name="number">ビンゴの番号です。</param>
    /// <returns>対応する背景ブラシです。</returns>
    public static SolidColorBrush Light(int number) => LightColors[Index(number)];

    /// <summary>番号に対応する文字色を返します。</summary>
    /// <param name="number">ビンゴの番号です。</param>
    /// <returns>対応する文字ブラシです。</returns>
    public static SolidColorBrush Ink(int number) => InkColors[Index(number)];

    /// <summary>番号から色の配列内の位置を求めます。</summary>
    /// <param name="number">ビンゴの番号です。</param>
    /// <returns>色の配列内の位置です。</returns>
    private static int Index(int number) => Math.Clamp((number - 1) / 15, 0, 4);

    /// <summary>色の各成分からブラシを作成します。</summary>
    /// <param name="red">赤の成分です。</param>
    /// <param name="green">緑の成分です。</param>
    /// <param name="blue">青の成分です。</param>
    /// <returns>作成したブラシです。</returns>
    private static SolidColorBrush Brush(byte red, byte green, byte blue) =>
        new(Color.FromArgb(255, red, green, blue));
}

