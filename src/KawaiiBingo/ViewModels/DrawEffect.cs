namespace KawaiiBingo.ViewModels;

/// <summary>抽選演出の中で画面に再生してほしい効果を表します。</summary>
public enum DrawEffect
{
    /// <summary>前回の演出を片付け、新しい抽選演出を始める合図です。</summary>
    Begin,

    /// <summary>ルーレットが一つ進んだときの小さな弾みです。</summary>
    Tick,

    /// <summary>おとりの番号でぴたっと止まったときの揺れです。</summary>
    FakeStop,

    /// <summary>止まった番号でどきどきと脈打つ、ための演出です。</summary>
    Heartbeat,

    /// <summary>決定したように見せる、ぬか喜びの演出です。</summary>
    FakeConfirm,

    /// <summary>決定と思わせた直後に、ようすが変わる急変の演出です。</summary>
    Twist,

    /// <summary>うさぎがこっそり手を出し始めたときの合図です。</summary>
    SneakStart,

    /// <summary>ボールを一つずらしたときのぐいっという揺れです。</summary>
    Nudge,

    /// <summary>魔法の杖が番号を変える演出です。</summary>
    Magic,

    /// <summary>磁石がボールを引き寄せる演出です。</summary>
    Magnet,

    /// <summary>プレゼントの箱から番号が現れる演出です。</summary>
    Gift,

    /// <summary>こっそりスタンプを押す演出です。</summary>
    Stamp,

    /// <summary>空飛ぶ円盤がボールをさらう演出です。</summary>
    Ufo,

    /// <summary>雲がボールを包む演出です。</summary>
    Cloud,

    /// <summary>ルーレットが逆回転する演出です。</summary>
    Reverse,

    /// <summary>にんじんで目をそらす演出です。</summary>
    Carrot,

    /// <summary>虹と星が飛び出す特別な演出です。</summary>
    Rainbow,

    /// <summary>本当の番号が決まったときの大きな演出です。</summary>
    Reveal,

    /// <summary>抽選に失敗したときに演出を元に戻す合図です。</summary>
    Cancel
}
