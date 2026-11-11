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

    /// <summary>どの演出も固有の効果を持ち、未抽選の候補だけを経て実際の結果に着地することを確認します。</summary>
    [TestMethod]
    public void EveryPatternUsesEligibleNumbersAndEndsAtTheWinner()
    {
        int[][] candidateSets =
        [
            Enumerable.Range(1, 75).ToArray(),
            [2, 17, 29, 60, 75],
            [4, 75]
        ];
        var firstEffects = new HashSet<DrawEffect?>();

        foreach (var pattern in Enum.GetValues<RiggedShowPattern>())
        {
            foreach (var candidates in candidateSets)
            {
                foreach (var winner in candidates)
                {
                    var script = RiggedShowScript.Create(candidates, winner, new Random(winner), pattern);
                    Assert.AreEqual(pattern, script.Pattern);
                    Assert.IsNotNull(script.Decoy);
                    Assert.AreNotEqual<int?>(winner, script.Decoy);
                    Assert.Contains(script.Decoy.Value, candidates);
                    Assert.AreEqual<int?>(winner, script.Steps[^1].Number);
                    Assert.IsTrue(script.Steps.All(step => step.Number is null || candidates.Contains(step.Number.Value)));
                    Assert.IsTrue(script.Steps.All(step => step.DurationMilliseconds > 0));
                    firstEffects.Add(script.Steps[0].Effect);
                }
            }
        }

        Assert.AreEqual(10, firstEffects.Count);
        Assert.IsFalse(firstEffects.Contains(null));
    }

    /// <summary>最後の一個でもおとりを捏造せず、十種類すべてを再生できることを確認します。</summary>
    [TestMethod]
    public void LastNumberNeedsNoDecoy()
    {
        foreach (var pattern in Enum.GetValues<RiggedShowPattern>())
        {
            var script = RiggedShowScript.Create(new[] { 42 }, 42, new Random(1), pattern);

            Assert.IsNull(script.Decoy);
            Assert.AreEqual<int?>(42, script.Steps[^1].Number);
            Assert.IsTrue(script.Steps.All(step => step.Number is null or 42));
        }
    }

    /// <summary>候補にない抽選結果から台本を作れないことを確認します。</summary>
    [TestMethod]
    public void WinnerMustBeAnEligibleCandidate()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            RiggedShowScript.Create(new[] { 3, 5 }, 7, new Random(1), RiggedShowPattern.Paw));
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
