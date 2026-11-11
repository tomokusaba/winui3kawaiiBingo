namespace KawaiiBingo.ViewModels;

/// <summary>
/// 当選番号が決まったあとに、いかにも操作されたように見せる演出の台本を作ります。
/// 当選番号そのものは変えず、表示の流れだけを組み立てます。
/// </summary>
internal sealed class RiggedShowScript
{
    /// <summary>おとりから本当の番号へずらすときの最大の段数です。</summary>
    private const int MaxNudgeSteps = 6;

    /// <summary>台本を作成します。</summary>
    /// <param name="decoy">いったん止まって見せるおとりの番号、または演出を省く場合の null です。</param>
    /// <param name="nudgePath">おとりから本当の番号までにボールに表示する番号の並びです。</param>
    private RiggedShowScript(int? decoy, IReadOnlyList<int> nudgePath)
    {
        Decoy = decoy;
        NudgePath = nudgePath;
    }

    /// <summary>いったん止まって見せるおとりの番号を取得します。</summary>
    public int? Decoy { get; }

    /// <summary>おとりから本当の番号までに表示する番号の並びを取得します。最後は必ず本当の番号です。</summary>
    public IReadOnlyList<int> NudgePath { get; }

    /// <summary>抽選結果に合わせた台本を作ります。</summary>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="winner">実際に抽選された番号です。</param>
    /// <param name="random">演出用の乱数です。</param>
    /// <returns>演出の台本です。</returns>
    public static RiggedShowScript Create(IReadOnlyList<int> candidates, int winner, Random random)
    {
        var others = candidates.Where(number => number != winner).ToArray();
        if (others.Length == 0)
        {
            return new RiggedShowScript(null, new[] { winner });
        }

        var close = others.Where(number => Math.Abs(number - winner) is >= 2 and <= MaxNudgeSteps).ToArray();
        var pool = close.Length > 0
            ? close
            : others.OrderBy(number => Math.Abs(number - winner)).Take(3).ToArray();
        var decoy = pool[random.Next(pool.Length)];

        var distance = winner - decoy;
        var steps = Math.Min(Math.Abs(distance), MaxNudgeSteps);
        var path = new List<int>(steps);
        for (var step = 1; step <= steps; step++)
        {
            var number = decoy + (int)Math.Round(distance * (double)step / steps, MidpointRounding.AwayFromZero);
            if (path.Count == 0 || path[^1] != number)
            {
                path.Add(number);
            }
        }

        return new RiggedShowScript(decoy, path);
    }
}
