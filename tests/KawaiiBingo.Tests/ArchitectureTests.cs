using KawaiiBingo.Domain;
using KawaiiBingo.UseCases.Games;

namespace KawaiiBingo.Tests;

/// <summary>抽選の内側の層が外側の実装に依存しないことを検証します。</summary>
[TestClass]
public sealed class ArchitectureTests
{
    /// <summary>ドメインとユースケースの参照方向を確認します。</summary>
    [TestMethod]
    public void InnerLayersDoNotReferenceOuterLayers()
    {
        var domainReferences = typeof(BingoGame).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToArray();
        var useCaseReferences = typeof(BingoGameService).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToArray();

        Assert.IsFalse(domainReferences.Any(name => name is not null && name.StartsWith("KawaiiBingo.", StringComparison.Ordinal)));
        Assert.IsFalse(useCaseReferences.Contains("KawaiiBingo.Infrastructure"));
        Assert.IsFalse(useCaseReferences.Contains("KawaiiBingo"));
    }
}
