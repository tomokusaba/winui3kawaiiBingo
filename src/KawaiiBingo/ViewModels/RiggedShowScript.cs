namespace KawaiiBingo.ViewModels;

/// <summary>抽選結果を変えずに再生する、いたずら演出の種類です。</summary>
internal enum RiggedShowPattern
{
    /// <summary>肉球で番号を押してずらす演出です。</summary>
    Paw,

    /// <summary>魔法の杖で番号を変える演出です。</summary>
    Magic,

    /// <summary>磁石で番号を引き寄せる演出です。</summary>
    Magnet,

    /// <summary>プレゼントの箱から番号が現れる演出です。</summary>
    Gift,

    /// <summary>こっそりスタンプを押す演出です。</summary>
    Stamp,

    /// <summary>空飛ぶ円盤が番号をさらう演出です。</summary>
    Ufo,

    /// <summary>雲に隠れて番号が入れ替わる演出です。</summary>
    Cloud,

    /// <summary>ルーレットが逆回転する演出です。</summary>
    Reverse,

    /// <summary>にんじんで目をそらす演出です。</summary>
    Carrot,

    /// <summary>虹と星が飛び出す特別な演出です。</summary>
    Rainbow
}

/// <summary>演出中の一場面を表します。</summary>
/// <param name="Effect">再生する効果、または表示だけを変える場合の null です。</param>
/// <param name="Number">表示する候補番号、または変更しない場合の null です。</param>
/// <param name="Message">マスコットの案内文です。</param>
/// <param name="ButtonText">抽選ボタンに表示する文字列です。</param>
/// <param name="DurationMilliseconds">次の場面までの時間です。</param>
internal sealed record RiggedShowStep(
    DrawEffect? Effect,
    int? Number,
    string Message,
    string ButtonText,
    int DurationMilliseconds);

/// <summary>確定済みの抽選結果を変えずに、いかさま風の演出を組み立てます。</summary>
internal sealed class RiggedShowScript
{
    /// <summary>演出の種類と、それぞれが選ばれる重みです。</summary>
    private static readonly (RiggedShowPattern Pattern, int Weight)[] PatternWeights =
    [
        (RiggedShowPattern.Paw, 14),
        (RiggedShowPattern.Magic, 13),
        (RiggedShowPattern.Magnet, 12),
        (RiggedShowPattern.Gift, 11),
        (RiggedShowPattern.Stamp, 10),
        (RiggedShowPattern.Ufo, 10),
        (RiggedShowPattern.Cloud, 9),
        (RiggedShowPattern.Reverse, 8),
        (RiggedShowPattern.Carrot, 8),
        (RiggedShowPattern.Rainbow, 5)
    ];

    /// <summary>演出の台本を作成します。</summary>
    /// <param name="pattern">演出の種類です。</param>
    /// <param name="decoy">いったん止まって見せる候補、または残りが一つの場合の null です。</param>
    /// <param name="steps">順番に再生する場面です。</param>
    private RiggedShowScript(RiggedShowPattern pattern, int? decoy, IReadOnlyList<RiggedShowStep> steps)
    {
        Pattern = pattern;
        Decoy = decoy;
        Steps = steps;
    }

    /// <summary>選ばれた演出の種類を取得します。</summary>
    public RiggedShowPattern Pattern { get; }

    /// <summary>いったん止まって見せる候補番号を取得します。</summary>
    public int? Decoy { get; }

    /// <summary>順番に再生する場面を取得します。</summary>
    public IReadOnlyList<RiggedShowStep> Steps { get; }

    /// <summary>演出の種類をランダムに選び、抽選結果に合わせた台本を作ります。</summary>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="winner">実際に抽選された番号です。</param>
    /// <param name="random">演出専用の乱数です。</param>
    /// <returns>演出の台本です。</returns>
    public static RiggedShowScript Create(IReadOnlyList<int> candidates, int winner, Random random) =>
        Create(candidates, winner, random, SelectPattern(random));

    /// <summary>重みに応じて演出の種類を一つ選びます。</summary>
    /// <param name="random">演出専用の乱数です。</param>
    /// <returns>選ばれた演出の種類です。</returns>
    internal static RiggedShowPattern SelectPattern(Random random)
    {
        var choice = random.Next(PatternWeights.Sum(item => item.Weight));
        foreach (var (pattern, weight) in PatternWeights)
        {
            if (choice < weight)
            {
                return pattern;
            }

            choice -= weight;
        }

        throw new InvalidOperationException("演出の種類を選択できませんでした。");
    }

    /// <summary>指定された種類と抽選結果に合わせた台本を作ります。</summary>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="winner">実際に抽選された番号です。</param>
    /// <param name="random">演出専用の乱数です。</param>
    /// <param name="pattern">再生する演出の種類です。</param>
    /// <returns>演出の台本です。</returns>
    internal static RiggedShowScript Create(
        IReadOnlyList<int> candidates,
        int winner,
        Random random,
        RiggedShowPattern pattern)
    {
        if (!candidates.Contains(winner))
        {
            throw new ArgumentException("抽選結果が候補に含まれていません。", nameof(winner));
        }

        var others = candidates.Where(number => number != winner).ToArray();
        var decoyPool = pattern == RiggedShowPattern.Paw
            ? others.OrderBy(number => Math.Abs(number - winner)).Take(4).ToArray()
            : others;
        int? decoy = decoyPool.Length == 0 ? null : decoyPool[random.Next(decoyPool.Length)];
        var extraPool = others.Where(number => number != decoy).ToArray();
        var extra = extraPool.Length == 0 ? decoy ?? winner : extraPool[random.Next(extraPool.Length)];
        var steps = new List<RiggedShowStep>();

        switch (pattern)
        {
            case RiggedShowPattern.Paw:
                steps.Add(new(DrawEffect.SneakStart, null, "…あっ、ちょっと まってね。", "ないしょ…", 500));
                foreach (var number in NudgePath(candidates, decoy ?? winner, winner))
                {
                    steps.Add(new(DrawEffect.Nudge, number, "ずりずり… ずりずり…", "ないしょ…", 330));
                }

                break;
            case RiggedShowPattern.Magic:
                steps.Add(new(DrawEffect.Magic, null, "おまじないを かけちゃお！", "ちちん…", 500));
                steps.Add(new(null, winner, "ちちんぷいっ！ きらりん♡", "ぷいっ！", 700));
                break;
            case RiggedShowPattern.Magnet:
                steps.Add(new(DrawEffect.Magnet, null, "あれ？ じしゃくが ぐいぐい…", "ひきよせ中", 500));
                steps.Add(new(null, winner, "すっ…と ひきよせちゃった！", "くっついた♡", 700));
                break;
            case RiggedShowPattern.Gift:
                steps.Add(new(DrawEffect.Gift, null, "プレゼントの なかみは…？", "どきどき…", 500));
                steps.Add(new(null, winner, "ぱかっ！ なんと びっくり♡", "ぱかっ！", 800));
                break;
            case RiggedShowPattern.Stamp:
                steps.Add(new(DrawEffect.Stamp, null, "こっそり はんこを…", "しーっ…", 350));
                steps.Add(new(null, winner, "ぽんっ！ …みてないよね？", "ぽんっ♡", 700));
                break;
            case RiggedShowPattern.Ufo:
                steps.Add(new(DrawEffect.Ufo, null, "おそらから なにかが…！？", "ふわふわ…", 600));
                steps.Add(new(null, winner, "さらわれて… もどってきた！", "ぴぴぴっ！", 700));
                break;
            case RiggedShowPattern.Cloud:
                steps.Add(new(DrawEffect.Cloud, null, "もくもく くもに かくれて…", "もくもく…", 500));
                steps.Add(new(null, winner, "くもが はれたら… あれれ？", "ふわり♡", 700));
                break;
            case RiggedShowPattern.Reverse:
                steps.Add(new(DrawEffect.Reverse, null, "えっ！？ ぎゃくに まわる！", "まきもどし！", 300));
                steps.Add(new(null, extra, "くるくる… もういっかい！", "くるくる…", 190));
                steps.Add(new(null, decoy ?? winner, "まだまだ とまらない…！", "くるくる…", 190));
                steps.Add(new(null, winner, "こんどこそ…？", "どきどき…", 650));
                break;
            case RiggedShowPattern.Carrot:
                steps.Add(new(DrawEffect.Carrot, null, "にんじん！ あっちを むいて♡", "よそみ中…", 500));
                steps.Add(new(null, winner, "そのすきに… ころんっ！", "ころんっ♡", 700));
                break;
            case RiggedShowPattern.Rainbow:
                steps.Add(new(DrawEffect.Rainbow, null, "わあっ！ にじいろの よかん！", "とくべつ演出", 400));
                steps.Add(new(null, extra, "きらきら… まだ とまらない！", "きらきら…", 330));
                steps.Add(new(null, decoy ?? winner, "まさか… もういっかい！？", "どきどき…", 330));
                steps.Add(new(null, winner, "にじいろの ひみつだよ♡", "きらりん！", 700));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pattern), pattern, "未知の演出です。");
        }

        return new RiggedShowScript(pattern, decoy, steps);
    }

    /// <summary>未抽選の候補だけを通って、おとりから実際の番号へ近づく順番を作ります。</summary>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="decoy">最初に表示する番号です。</param>
    /// <param name="winner">実際に抽選された番号です。</param>
    /// <returns>順番に表示する候補番号です。</returns>
    private static IReadOnlyList<int> NudgePath(IReadOnlyList<int> candidates, int decoy, int winner)
    {
        var direction = Math.Sign(winner - decoy);
        var path = candidates
            .Where(number => (number - decoy) * direction > 0 && (winner - number) * direction >= 0)
            .OrderBy(number => (number - decoy) * direction)
            .Take(4)
            .ToList();
        if (path.Count == 0 || path[^1] != winner)
        {
            path.Add(winner);
        }

        return path;
    }
}
