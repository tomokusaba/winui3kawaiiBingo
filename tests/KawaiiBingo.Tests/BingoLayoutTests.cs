using KawaiiBingo.Layout;

namespace KawaiiBingo.Tests;

/// <summary>画面を崩さずに拡大し、利用できる領域を使う配置を検証します。</summary>
[TestClass]
public sealed class BingoLayoutTests
{
    /// <summary>横長の全画面では、同じ倍率で拡大した画面が縦横とも表示領域を使い切ることを確認します。</summary>
    /// <param name="width">表示領域の幅です。</param>
    /// <param name="height">表示領域の高さです。</param>
    /// <param name="expectedScale">文字と演出に適用する拡大率です。</param>
    [TestMethod]
    [DataRow(1280d, 720d, 1d)]
    [DataRow(1920d, 1080d, 1.5d)]
    [DataRow(2560d, 1440d, 2d)]
    [DataRow(3840d, 2160d, 3d)]
    [DataRow(3440d, 1440d, 2d)]
    [DataRow(1280d, 960d, 1d)]
    public void WideScreenFillsViewportWithoutClipping(double width, double height, double expectedScale)
    {
        var layout = BingoLayout.Fit(width, height, 0);
        var scale = width / layout.Width;

        Assert.IsTrue(layout.IsWide);
        Assert.AreEqual(height, layout.Height * scale, 0.001);
        Assert.IsGreaterThanOrEqualTo(1280, layout.Width);
        Assert.IsGreaterThanOrEqualTo(720, layout.Height);
        Assert.IsGreaterThanOrEqualTo(0.78, scale);
        Assert.AreEqual(expectedScale, scale, 0.001);
    }

    /// <summary>狭い画面では番号を隠さず縦に並べ、必要な高さをスクロールで確保することを確認します。</summary>
    [TestMethod]
    public void NarrowScreenKeepsReadableStackedContent()
    {
        var layout = BingoLayout.Fit(650, 440, 0);

        Assert.IsFalse(layout.IsWide);
        Assert.AreEqual(720, layout.Width, 0.001);
        Assert.AreEqual(1180, layout.Height, 0.001);
    }

    /// <summary>縦長の大画面では縦の配置を保って、画面下まで表示を広げることを確認します。</summary>
    [TestMethod]
    public void PortraitScreenUsesItsFullHeight()
    {
        var layout = BingoLayout.Fit(1080, 1920, 0);

        Assert.IsFalse(layout.IsWide);
        Assert.AreEqual(1920, layout.Height * 1080 / layout.Width, 0.001);
    }

    /// <summary>高さの低い画面でも操作部を過度に縮小せず、必要ならスクロールすることを確認します。</summary>
    [TestMethod]
    public void ShortScreenDoesNotShrinkControlsTooFar()
    {
        var layout = BingoLayout.Fit(1200, 440, 0);
        var scale = 1200 / layout.Width;

        Assert.AreEqual(0.78, scale, 0.001);
        Assert.IsGreaterThan(440, layout.Height * scale);
    }

    /// <summary>エラー表示が増えても、抽選ボタンと番号ボードに必要な領域を確保することを確認します。</summary>
    [TestMethod]
    public void ErrorMessageAddsSpaceWithoutCoveringControls()
    {
        var layout = BingoLayout.Fit(1920, 1080, 120);

        Assert.IsGreaterThanOrEqualTo(840, layout.Height);
        Assert.AreEqual(1080, layout.Height * 1920 / layout.Width, 0.001);
    }
}
