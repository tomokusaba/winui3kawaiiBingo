using KawaiiBingo.Domain;
using KawaiiBingo.UseCases.Abstractions;

namespace KawaiiBingo.UseCases.Games;

/// <summary>抽選、取り消し、リセットと保存を一つずつ実行します。</summary>
public sealed class BingoGameService : IDisposable
{
    /// <summary>抽選状態の保存先です。</summary>
    private readonly IBingoGameRepository _repository;

    /// <summary>抽選に使用する乱数の提供元です。</summary>
    private readonly IRandomSource _random;

    /// <summary>同時操作による履歴の競合を防ぎます。</summary>
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>抽選サービスを作成します。</summary>
    /// <param name="repository">抽選状態の保存先です。</param>
    /// <param name="random">抽選に使用する乱数の提供元です。</param>
    public BingoGameService(IBingoGameRepository repository, IRandomSource random)
    {
        _repository = repository;
        _random = random;
    }

    /// <summary>現在の抽選状態を取得します。</summary>
    /// <param name="cancellationToken">処理のキャンセルを通知します。</param>
    /// <returns>現在の抽選状態です。</returns>
    public async Task<BingoSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return new BingoSnapshot(await _repository.LoadAsync(cancellationToken).ConfigureAwait(false));
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>番号を一つ抽選して保存します。</summary>
    /// <param name="cancellationToken">処理のキャンセルを通知します。</param>
    /// <returns>抽選後の状態です。</returns>
    public async Task<BingoSnapshot> DrawAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var game = await _repository.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (!game.IsComplete)
            {
                game.Draw(_random);
                await _repository.SaveAsync(game, cancellationToken).ConfigureAwait(false);
            }

            return new BingoSnapshot(game);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>最後の抽選を取り消して保存します。</summary>
    /// <param name="cancellationToken">処理のキャンセルを通知します。</param>
    /// <returns>取り消し後の状態です。</returns>
    public async Task<BingoSnapshot> UndoAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var game = await _repository.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (game.UndoLastDraw() is not null)
            {
                await _repository.SaveAsync(game, cancellationToken).ConfigureAwait(false);
            }

            return new BingoSnapshot(game);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>抽選履歴をリセットして保存します。</summary>
    /// <param name="cancellationToken">処理のキャンセルを通知します。</param>
    /// <returns>リセット後の状態です。</returns>
    public async Task<BingoSnapshot> ResetAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var game = await _repository.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (game.DrawnCount > 0)
            {
                game.Reset();
                await _repository.SaveAsync(game, cancellationToken).ConfigureAwait(false);
            }

            return new BingoSnapshot(game);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>操作の同期に使用したリソースを解放します。</summary>
    public void Dispose() => _gate.Dispose();
}
