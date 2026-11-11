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

/// <summary>確定済みの抽選結果を変えずに、0〜3回のいかさま風演出とためを組み立てます。</summary>
internal sealed class RiggedShowScript
{
    /// <summary>一回の抽選で行う、いかさまの最大回数です。</summary>
    public const int MaxTricks = 3;

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

    /// <summary>いかさまの回数（0〜3回）と、それぞれが選ばれる重みです。</summary>
    private static readonly int[] TrickCountWeights = [20, 35, 30, 15];

    /// <summary>演出の台本を作成します。</summary>
    /// <param name="patterns">順番に再生するいかさまの種類です。</param>
    /// <param name="stops">いったん止まって見せる番号の順番です。最後は実際の番号です。</param>
    /// <param name="steps">順番に再生する場面です。</param>
    private RiggedShowScript(IReadOnlyList<RiggedShowPattern> patterns, IReadOnlyList<int> stops, IReadOnlyList<RiggedShowStep> steps)
    {
        Patterns = patterns;
        Stops = stops;
        Steps = steps;
    }

    /// <summary>順番に再生するいかさまの種類を取得します。空の場合はいかさまなしです。</summary>
    public IReadOnlyList<RiggedShowPattern> Patterns { get; }

    /// <summary>止まって見せる番号の順番を取得します。最後の番号は実際の抽選結果です。</summary>
    public IReadOnlyList<int> Stops { get; }

    /// <summary>ルーレットが最初に止まる番号を取得します。</summary>
    public int FirstStop => Stops[0];

    /// <summary>ルーレットが止まった後に順番に再生する場面を取得します。</summary>
    public IReadOnlyList<RiggedShowStep> Steps { get; }

    /// <summary>いかさまの回数と種類をランダムに選び、抽選結果に合わせた台本を作ります。</summary>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="winner">実際に抽選された番号です。</param>
    /// <param name="random">演出専用の乱数です。</param>
    /// <returns>演出の台本です。</returns>
    public static RiggedShowScript Create(IReadOnlyList<int> candidates, int winner, Random random)
    {
        var count = SelectTrickCount(random);
        var patterns = new List<RiggedShowPattern>();
        for (var index = 0; index < count; index++)
        {
            patterns.Add(SelectPattern(random, patterns));
        }

        return Create(candidates, winner, random, patterns);
    }

    /// <summary>重みに応じて、いかさまの回数を0〜3回から選びます。</summary>
    /// <param name="random">演出専用の乱数です。</param>
    /// <returns>いかさまの回数です。</returns>
    internal static int SelectTrickCount(Random random)
    {
        var choice = random.Next(TrickCountWeights.Sum());
        for (var count = 0; count < TrickCountWeights.Length; count++)
        {
            if (choice < TrickCountWeights[count])
            {
                return count;
            }

            choice -= TrickCountWeights[count];
        }

        throw new InvalidOperationException("いかさまの回数を選択できませんでした。");
    }

    /// <summary>重みに応じて演出の種類を一つ選びます。</summary>
    /// <param name="random">演出専用の乱数です。</param>
    /// <returns>選ばれた演出の種類です。</returns>
    internal static RiggedShowPattern SelectPattern(Random random) => SelectPattern(random, []);

    /// <summary>まだ使っていない演出の中から、重みに応じて一つ選びます。</summary>
    /// <param name="random">演出専用の乱数です。</param>
    /// <param name="used">同じ抽選ですでに使った演出です。</param>
    /// <returns>選ばれた演出の種類です。</returns>
    internal static RiggedShowPattern SelectPattern(Random random, IReadOnlyCollection<RiggedShowPattern> used)
    {
        var available = PatternWeights.Where(item => !used.Contains(item.Pattern)).ToArray();
        var choice = random.Next(available.Sum(item => item.Weight));
        foreach (var (pattern, weight) in available)
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
    /// <param name="patterns">順番に再生するいかさまの種類です。候補が足りない場合は先頭から使える分だけ再生します。</param>
    /// <returns>演出の台本です。</returns>
    internal static RiggedShowScript Create(
        IReadOnlyList<int> candidates,
        int winner,
        Random random,
        IReadOnlyList<RiggedShowPattern> patterns)
    {
        if (!candidates.Contains(winner))
        {
            throw new ArgumentException("抽選結果が候補に含まれていません。", nameof(winner));
        }

        var others = candidates.Where(number => number != winner).ToArray();
        var played = patterns.Take(Math.Min(MaxTricks, others.Length)).ToArray();
        var stops = new int[played.Length + 1];
        stops[^1] = winner;
        for (var index = played.Length - 1; index >= 0; index--)
        {
            var next = stops[index + 1];
            var pool = others.Where(number => number != next && !stops[(index + 1)..^1].Contains(number)).ToArray();
            if (played[index] == RiggedShowPattern.Paw)
            {
                pool = pool.OrderBy(number => Math.Abs(number - next)).Take(4).ToArray();
            }

            stops[index] = pool[random.Next(pool.Length)];
        }

        var steps = new List<RiggedShowStep>();
        for (var index = 0; index < stops.Length; index++)
        {
            AddHold(steps, stops[index], candidates.Count == 1, random);
            if (index < played.Length)
            {
                steps.Add(new(DrawEffect.Twist, null, Pick(random, TwistMessages), "！？", 650));
                AddTrick(steps, played[index], candidates, stops[index], stops[index + 1], random);
            }
        }

        return new RiggedShowScript(played, stops, steps);
    }

    /// <summary>いかさまの直前に出す、急変の案内文です。</summary>
    private static readonly string[] TwistMessages =
    [
        "…と おもったら！？",
        "…あれれ？ ちょっと まって！",
        "えっ、 まだ おわらない！？",
        "…なんだか ようすが へん…！"
    ];

    /// <summary>止まった番号で「決定かも？」と思わせる、ためを追加します。</summary>
    /// <param name="steps">場面を追加する一覧です。</param>
    /// <param name="number">止まって見せる番号です。</param>
    /// <param name="isLastNumber">最後の一個で、変わる余地がない場合は真です。</param>
    /// <param name="random">演出専用の乱数です。</param>
    private static void AddHold(List<RiggedShowStep> steps, int number, bool isLastNumber, Random random)
    {
        var name = $"{Letter(number)} の {number}";
        if (isLastNumber)
        {
            steps.Add(new(DrawEffect.FakeStop, number, "さいごの ひとつ…！", "どきどき…", 700));
            steps.Add(new(DrawEffect.Heartbeat, null, "……", "どきどき…", 700));
            return;
        }

        steps.Add(new(DrawEffect.FakeStop, number, $"{name} に けってい…？", "けってい…？", Between(random, 850, 1250)));
        switch (random.Next(3))
        {
            case 0:
                steps.Add(new(DrawEffect.Heartbeat, null, "どきどき…", "どきどき…", Between(random, 750, 1000)));
                steps.Add(new(DrawEffect.Heartbeat, null, "どきどき… どきどき…", "どきどき…", Between(random, 850, 1150)));
                break;
            case 1:
                steps.Add(new(DrawEffect.Heartbeat, null, "……", "……", Between(random, 700, 950)));
                steps.Add(new(DrawEffect.FakeConfirm, null, $"{name} で けってい～♡", "けってい♡", Between(random, 1000, 1350)));
                break;
            default:
                steps.Add(new(DrawEffect.Heartbeat, null, "……", "……", Between(random, 550, 700)));
                steps.Add(new(DrawEffect.Heartbeat, null, "………", "……", Between(random, 700, 900)));
                steps.Add(new(DrawEffect.Heartbeat, null, "…………", "……", Between(random, 850, 1150)));
                break;
        }
    }

    /// <summary>一回分のいかさまを追加し、指定された番号に着地させます。</summary>
    /// <param name="steps">場面を追加する一覧です。</param>
    /// <param name="pattern">再生するいかさまの種類です。</param>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="from">いかさま前に止まっていた番号です。</param>
    /// <param name="to">いかさま後に着地する番号です。</param>
    /// <param name="random">演出専用の乱数です。</param>
    private static void AddTrick(
        List<RiggedShowStep> steps,
        RiggedShowPattern pattern,
        IReadOnlyList<int> candidates,
        int from,
        int to,
        Random random)
    {
        var extraPool = candidates.Where(number => number != from && number != to).ToArray();
        var extra = extraPool.Length == 0 ? from : extraPool[random.Next(extraPool.Length)];

        switch (pattern)
        {
            case RiggedShowPattern.Paw:
                steps.Add(new(DrawEffect.SneakStart, null, "…あっ、ちょっと まってね。", "ないしょ…", 550));
                foreach (var number in NudgePath(candidates, from, to))
                {
                    steps.Add(new(DrawEffect.Nudge, number, "ずりずり… ずりずり…", "ないしょ…", 360));
                }

                break;
            case RiggedShowPattern.Magic:
                steps.Add(new(DrawEffect.Magic, null, "おまじないを かけちゃお！", "ちちん…", 650));
                steps.Add(new(null, to, "ちちんぷいっ！ きらりん♡", "ぷいっ！", 700));
                break;
            case RiggedShowPattern.Magnet:
                steps.Add(new(DrawEffect.Magnet, null, "あれ？ じしゃくが ぐいぐい…", "ひきよせ中", 650));
                steps.Add(new(null, to, "すっ…と ひきよせちゃった！", "くっついた♡", 700));
                break;
            case RiggedShowPattern.Gift:
                steps.Add(new(DrawEffect.Gift, null, "プレゼントの なかみは…？", "どきどき…", 700));
                steps.Add(new(null, to, "ぱかっ！ なんと びっくり♡", "ぱかっ！", 800));
                break;
            case RiggedShowPattern.Stamp:
                steps.Add(new(DrawEffect.Stamp, null, "こっそり はんこを…", "しーっ…", 450));
                steps.Add(new(null, to, "ぽんっ！ …みてないよね？", "ぽんっ♡", 700));
                break;
            case RiggedShowPattern.Ufo:
                steps.Add(new(DrawEffect.Ufo, null, "おそらから なにかが…！？", "ふわふわ…", 700));
                steps.Add(new(null, to, "さらわれて… もどってきた！", "ぴぴぴっ！", 700));
                break;
            case RiggedShowPattern.Cloud:
                steps.Add(new(DrawEffect.Cloud, null, "もくもく くもに かくれて…", "もくもく…", 700));
                steps.Add(new(null, to, "くもが はれたら… あれれ？", "ふわり♡", 700));
                break;
            case RiggedShowPattern.Reverse:
                steps.Add(new(DrawEffect.Reverse, null, "えっ！？ ぎゃくに まわる！", "まきもどし！", 350));
                steps.Add(new(null, extra, "くるくる… もういっかい！", "くるくる…", 200));
                steps.Add(new(null, from, "まだまだ とまらない…！", "くるくる…", 260));
                steps.Add(new(null, to, "こんどこそ…？", "どきどき…", 450));
                break;
            case RiggedShowPattern.Carrot:
                steps.Add(new(DrawEffect.Carrot, null, "にんじん！ あっちを むいて♡", "よそみ中…", 650));
                steps.Add(new(null, to, "そのすきに… ころんっ！", "ころんっ♡", 700));
                break;
            case RiggedShowPattern.Rainbow:
                steps.Add(new(DrawEffect.Rainbow, null, "わあっ！ にじいろの よかん！", "とくべつ演出", 450));
                steps.Add(new(null, extra, "きらきら… まだ とまらない！", "きらきら…", 330));
                steps.Add(new(null, from, "まさか… もういっかい！？", "どきどき…", 330));
                steps.Add(new(null, to, "にじいろの ひみつだよ♡", "きらりん！", 600));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pattern), pattern, "未知の演出です。");
        }
    }

    /// <summary>未抽選の候補だけを通って、止まっていた番号から次の番号へ近づく順番を作ります。</summary>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="from">最初に表示されている番号です。</param>
    /// <param name="to">最後に着地する番号です。</param>
    /// <returns>順番に表示する候補番号です。</returns>
    private static IReadOnlyList<int> NudgePath(IReadOnlyList<int> candidates, int from, int to)
    {
        var direction = Math.Sign(to - from);
        var path = candidates
            .Where(number => (number - from) * direction > 0 && (to - number) * direction >= 0)
            .OrderBy(number => (number - from) * direction)
            .Take(4)
            .ToList();
        if (path.Count == 0 || path[^1] != to)
        {
            path.Add(to);
        }

        return path;
    }

    /// <summary>指定された範囲から時間をランダムに選び、ためのタイミングを毎回変えます。</summary>
    /// <param name="random">演出専用の乱数です。</param>
    /// <param name="minimum">最短の時間です。</param>
    /// <param name="maximum">最長の時間（含まない）です。</param>
    /// <returns>選ばれた時間です。</returns>
    private static int Between(Random random, int minimum, int maximum) => minimum + random.Next(maximum - minimum);

    /// <summary>文言の候補から一つ選びます。</summary>
    /// <param name="random">演出専用の乱数です。</param>
    /// <param name="messages">文言の候補です。</param>
    /// <returns>選ばれた文言です。</returns>
    private static string Pick(Random random, string[] messages) => messages[random.Next(messages.Length)];

    /// <summary>番号に対応する BINGO の文字を返します。</summary>
    /// <param name="number">1〜75の番号です。</param>
    /// <returns>対応する文字です。</returns>
    private static char Letter(int number) => "BINGO"[(number - 1) / 15];
}
