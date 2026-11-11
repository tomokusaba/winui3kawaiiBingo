using System.Security.Cryptography;
using KawaiiBingo.Domain;

namespace KawaiiBingo.Infrastructure.Randomness;

/// <summary>暗号学的な乱数を使用して抽選します。</summary>
internal sealed class CryptoRandomSource : IRandomSource
{
    /// <summary>ゼロ以上、指定値未満の乱数を返します。</summary>
    /// <param name="exclusiveMaximum">乱数の上限（この値は含みません）。</param>
    /// <returns>生成された整数です。</returns>
    public int Next(int exclusiveMaximum) => RandomNumberGenerator.GetInt32(exclusiveMaximum);
}
