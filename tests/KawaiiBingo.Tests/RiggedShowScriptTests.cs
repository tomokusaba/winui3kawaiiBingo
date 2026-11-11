using KawaiiBingo.ViewModels;

namespace KawaiiBingo.Tests;

/// <summary>抽選結果を変えない、いたずら演出の台本を検証します。</summary>
[TestClass]
public sealed class RiggedShowScriptTests
{
    /// <summary>十種類すべてが選ばれ、虹の演出がときどき現れることを確認します。</summary>
    [TestMethod]
    public void RandomSelectionCanReachEveryPattern()
    {
        var choices = Enumerable.Range(0, 100)
            .Select(value => RiggedShowScript.SelectPattern(new FixedRandom(value)))
            .ToArray();

        CollectionAssert.AreEquivalent(Enum.GetValues<RiggedShowPattern>(), choices.Distinct().ToArray());
        Assert.AreEqual(10, choices.Distinct().Count());
        Assert.AreEqual(5, choices.Count(pattern => pattern == RiggedShowPattern.Rainbow));
    }

    /// <summary>いかさまの回数が0〜3回のすべてになりうることを確認します。</summary>
    [TestMethod]
    public void TrickCountRangesFromZeroToThree()
    {
        var counts = Enumerable.Range(0, 100)
            .Select(value => RiggedShowScript.SelectTrickCount(new FixedRandom(value)))
            .ToArray();

        CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3 }, counts.Distinct().ToArray());
    }

    /// <summary>同じ抽選の中では、同じいかさまを繰り返さないことを確認します。</summary>
    [TestMethod]
    public void RandomScriptsNeverRepeatAPatternAndStayWithinThreeTricks()
    {
        var candidates = Enumerable.Range(1, 75).ToArray();
        var seen = new HashSet<int>();
        for (var seed = 0; seed < 300; seed++)
        {
            var script = RiggedShowScript.Create(candidates, 30, new Random(seed));
            Assert.IsLessThanOrEqualTo(3, script.Patterns.Count);
            Assert.AreEqual(script.Patterns.Count, script.Patterns.Distinct().Count());
            seen.Add(script.Patterns.Count);
        }

        CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3 }, seen.ToArray());
    }

    /// <summary>どの演出の組み合わせも、ための後に急変し、未抽選の候補だけを経て実際の結果に着地することを確認します。</summary>
    [TestMethod]
    public void EveryPatternChainHoldsTwistsAndEndsAtTheWinner()
    {
        int[][] candidateSets =
        [
            Enumerable.Range(1, 75).ToArray(),
            [2, 17, 29, 60, 75],
            [4, 75]
        ];
        var all = Enum.GetValues<RiggedShowPattern>();
        var chains = new List<RiggedShowPattern[]> { Array.Empty<RiggedShowPattern>() };
        chains.AddRange(all.Select(pattern => new[] { pattern }));
        chains.AddRange(all.Select((pattern, index) => new[] { pattern, all[(index + 3) % all.Length], all[(index + 7) % all.Length] }));
        var trickEffects = new HashSet<DrawEffect?>();

        foreach (var chain in chains)
        {
            foreach (var candidates in candidateSets)
            {
                foreach (var winner in candidates)
                {
                    var script = RiggedShowScript.Create(candidates, winner, new Random(winner), chain);
                    var expectedTricks = Math.Min(chain.Length, candidates.Length - 1);

                    CollectionAssert.AreEqual(chain.Take(expectedTricks).ToArray(), script.Patterns.ToArray());
                    Assert.HasCount(expectedTricks + 1, script.Stops);
                    Assert.AreEqual(winner, script.Stops[^1]);
                    Assert.IsTrue(script.Stops.Take(expectedTricks).All(stop => stop != winner && candidates.Contains(stop)));
                    Assert.IsTrue(script.Stops.Zip(script.Stops.Skip(1)).All(pair => pair.First != pair.Second));
                    Assert.AreEqual(expectedTricks + 1, script.Steps.Count(step => step.Effect == DrawEffect.FakeStop));
                    Assert.AreEqual(expectedTricks, script.Steps.Count(step => step.Effect == DrawEffect.Twist));
                    Assert.AreEqual<int?>(winner, script.Steps.Last(step => step.Number is not null).Number);
                    Assert.IsTrue(script.Steps.All(step => step.Number is null || candidates.Contains(step.Number.Value)));
                    Assert.IsTrue(script.Steps.All(step => step.DurationMilliseconds > 0));
                    Assert.IsFalse(script.Steps.SkipWhile(step => step.Effect != DrawEffect.FakeStop || step.Number != winner)
                        .Any(step => step.Effect == DrawEffect.Twist || (step.Number is int number && number != winner)));
                    trickEffects.UnionWith(script.Steps
                        .SkipWhile(step => step.Effect != DrawEffect.Twist)
                        .Skip(1)
                        .Take(1)
                        .Select(step => step.Effect));
                }
            }
        }

        Assert.AreEqual(10, trickEffects.Count);
        Assert.IsFalse(trickEffects.Contains(null));
    }

    /// <summary>最後の一個ではおとりを捏造せず、いかさまなしで着地することを確認します。</summary>
    [TestMethod]
    public void LastNumberNeedsNoDecoy()
    {
        foreach (var pattern in Enum.GetValues<RiggedShowPattern>())
        {
            var script = RiggedShowScript.Create(new[] { 42 }, 42, new Random(1), [pattern]);

            Assert.IsEmpty(script.Patterns);
            CollectionAssert.AreEqual(new[] { 42 }, script.Stops.ToArray());
            Assert.IsTrue(script.Steps.All(step => step.Number is null or 42));
        }
    }
    /// <summary>候補にない抽選結果から台本を作れないことを確認します。</summary>
    [TestMethod]
    public void WinnerMustBeAnEligibleCandidate()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            RiggedShowScript.Create(new[] { 3, 5 }, 7, new Random(1), [RiggedShowPattern.Paw]));
    }

    /// <summary>演出の抽選位置を固定するテスト用の乱数です。</summary>
    /// <param name="value">返す抽選位置です。</param>
    private sealed class FixedRandom(int value) : Random
    {
        /// <summary>指定された範囲の決められた位置を返します。</summary>
        /// <param name="maxValue">範囲の上限です。</param>
        /// <returns>固定された位置です。</returns>
        public override int Next(int maxValue) => value < maxValue
            ? value
            : throw new ArgumentOutOfRangeException(nameof(maxValue));
    }
}
