using KawaiiBingo.Domain;

namespace KawaiiBingo.UseCases.Abstractions;

/// <summary>ビンゴの抽選状態を読み書きします。</summary>
public interface IBingoGameRepository
{
    /// <summary>保存済みの抽選状態を読み込みます。</summary>
    /// <param name="cancellationToken">読み込みのキャンセルを通知します。</param>
    /// <returns>保存された状態、または初期状態です。</returns>
    Task<BingoGame> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>抽選状態を保存します。</summary>
    /// <param name="game">保存する抽選状態です。</param>
    /// <param name="cancellationToken">保存のキャンセルを通知します。</param>
    /// <returns>保存の完了を表すタスクです。</returns>
    Task SaveAsync(BingoGame game, CancellationToken cancellationToken = default);
}
