using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KawaiiBingo.Tests;

/// <summary>日本語の XML コメントが各メンバーに付いていることを検証します。</summary>
[TestClass]
public sealed class DocumentationTests
{
    /// <summary>手書きの型とメンバーに日本語の概要があることを確認します。</summary>
    [TestMethod]
    public void EveryMemberHasJapaneseSummary()
    {
        var root = FindRepositoryRoot();
        var errors = new List<string>();
        var options = new CSharpParseOptions(documentationMode: DocumentationMode.Parse);
        var japanese = new Regex(@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]");

        foreach (var sourceDirectory in new[] { "src", "tests" })
        {
            var files = Directory.EnumerateFiles(Path.Combine(root, sourceDirectory), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Split(Path.DirectorySeparatorChar)
                    .Any(part => part.Equals("obj", StringComparison.OrdinalIgnoreCase)
                        || part.Equals("bin", StringComparison.OrdinalIgnoreCase)));

            foreach (var path in files)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), options, path);
                var members = tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>()
                    .Where(member => member is not BaseNamespaceDeclarationSyntax and not GlobalStatementSyntax);
                foreach (var member in members)
                {
                    var summaries = member.GetLeadingTrivia()
                        .Select(trivia => trivia.GetStructure())
                        .OfType<DocumentationCommentTriviaSyntax>()
                        .SelectMany(comment => comment.DescendantNodes().OfType<XmlElementSyntax>())
                        .Where(element => element.StartTag.Name.LocalName.ValueText == "summary");
                    if (!summaries.Any(summary => japanese.IsMatch(summary.Content.ToString())))
                    {
                        var line = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        errors.Add($"{Path.GetRelativePath(root, path)}:{line}");
                    }
                }
            }
        }

        Assert.IsEmpty(errors, $"日本語の概要がありません: {string.Join(", ", errors)}");
    }

    /// <summary>ソリューションファイルを基準にリポジトリのルートを探します。</summary>
    /// <returns>リポジトリのルートディレクトリです。</returns>
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "KawaiiBingo.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("ソリューションのルートが見つかりません。");
    }
}
