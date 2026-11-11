namespace KawaiiBingo.Domain;

/// <summary>抽選に使用する乱数を提供します。</summary>
public interface IRandomSource
{
    /// <summary>ゼロ以上、指定値未満の整数を返します。</summary>
    /// <param name="exclusiveMaximum">返す値の上限（この値は含みません）。</param>
    /// <returns>抽選された整数です。</returns>
    int Next(int exclusiveMaximum);
}
