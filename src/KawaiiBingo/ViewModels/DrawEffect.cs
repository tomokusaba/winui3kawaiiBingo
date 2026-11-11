namespace KawaiiBingo.ViewModels;

/// <summary>抽選演出の中で画面に再生してほしい効果を表します。</summary>
public enum DrawEffect
{
    /// <summary>ルーレットが一つ進んだときの小さな弾みです。</summary>
    Tick,

    /// <summary>おとりの番号でぴたっと止まったときの揺れです。</summary>
    FakeStop,

    /// <summary>うさぎがこっそり手を出し始めたときの合図です。</summary>
    SneakStart,

    /// <summary>ボールを一つずらしたときのぐいっという揺れです。</summary>
    Nudge,

    /// <summary>本当の番号が決まったときの大きな演出です。</summary>
    Reveal,

    /// <summary>抽選に失敗したときに演出を元に戻す合図です。</summary>
    Cancel
}
