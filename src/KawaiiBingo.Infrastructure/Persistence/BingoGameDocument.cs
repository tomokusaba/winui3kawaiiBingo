namespace KawaiiBingo.Infrastructure.Persistence;

/// <summary>抽選履歴の保存形式を表します。</summary>
internal sealed class BingoGameDocument
{
    /// <summary>保存形式のバージョンを取得または設定します。</summary>
    public int Version { get; set; }

    /// <summary>抽選順に並んだ番号を取得または設定します。</summary>
    public int[]? DrawnNumbers { get; set; }
}
