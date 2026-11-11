namespace KawaiiBingo.Layout;

/// <summary>縦横比を保って拡大する画面の論理サイズと配置を表します。</summary>
/// <param name="Width">画面の論理幅です。</param>
/// <param name="Height">画面の論理高さです。</param>
/// <param name="IsWide">抽選と番号ボードを左右に配置するかどうかです。</param>
internal readonly record struct BingoLayout(double Width, double Height, bool IsWide)
{
    /// <summary>表示領域を使い切り、操作部を小さくしすぎない画面サイズを求めます。</summary>
    /// <param name="width">利用できる表示領域の幅です。</param>
    /// <param name="height">利用できる表示領域の高さです。</param>
    /// <param name="errorHeight">エラー表示に必要な追加の高さです。</param>
    /// <returns>拡大前の画面サイズと配置です。</returns>
    public static BingoLayout Fit(double width, double height, double errorHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegative(errorHeight);

        var isWide = width >= 1000 && width / height >= 1.2;
        var minimumHeight = (isWide ? 720 : 1180) + errorHeight;
        var widthScale = width / (isWide ? 1280 : 720);
        var scale = isWide
            ? Math.Max(0.78, Math.Min(widthScale, height / minimumHeight))
            : widthScale;

        return new BingoLayout(width / scale, Math.Max(minimumHeight, height / scale), isWide);
    }
}
