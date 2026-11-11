using System.Text.Json;
using KawaiiBingo.Domain;
using KawaiiBingo.UseCases.Abstractions;
using Microsoft.Extensions.Logging;

namespace KawaiiBingo.Infrastructure.Persistence;

/// <summary>抽選履歴を JSON ファイルに保存します。</summary>
internal sealed class JsonBingoGameRepository : IBingoGameRepository
{
    /// <summary>現在の保存形式のバージョンです。</summary>
    private const int CurrentVersion = 1;

    /// <summary>抽選履歴ファイルのパスです。</summary>
    private readonly string _filePath;

    /// <summary>一時ファイルの後始末に失敗した場合の記録先です。</summary>
    private readonly ILogger<JsonBingoGameRepository> _logger;

    /// <summary>保存先を指定してリポジトリを作成します。</summary>
    /// <param name="dataDirectory">抽選履歴を置くディレクトリです。</param>
    /// <param name="logger">ログの記録先です。</param>
    public JsonBingoGameRepository(string dataDirectory, ILogger<JsonBingoGameRepository> logger)
    {
        _filePath = Path.Combine(dataDirectory, "game.json");
        _logger = logger;
    }

    /// <summary>保存された抽選履歴を読み込みます。</summary>
    /// <param name="cancellationToken">読み込みのキャンセルを通知します。</param>
    /// <returns>復元された抽選状態です。</returns>
    public async Task<BingoGame> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await using var stream = new FileStream(
                _filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            var document = await JsonSerializer.DeserializeAsync(
                stream, PersistenceJsonContext.Default.BingoGameDocument, cancellationToken).ConfigureAwait(false);
            if (document is null || document.Version != CurrentVersion || document.DrawnNumbers is null)
            {
                throw new InvalidDataException("抽選履歴の形式を読み取れません。");
            }

            try
            {
                return BingoGame.Restore(document.DrawnNumbers);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("抽選履歴に不正な番号があります。", exception);
            }
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("抽選履歴の JSON を読み取れません。", exception);
        }
        catch (FileNotFoundException)
        {
            return new BingoGame();
        }
        catch (DirectoryNotFoundException)
        {
            return new BingoGame();
        }
    }

    /// <summary>抽選履歴を一時ファイル経由で安全に保存します。</summary>
    /// <param name="game">保存する抽選状態です。</param>
    /// <param name="cancellationToken">保存のキャンセルを通知します。</param>
    /// <returns>保存の完了を表すタスクです。</returns>
    public async Task SaveAsync(BingoGame game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        cancellationToken.ThrowIfCancellationRequested();
        var directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $"game.{Guid.NewGuid():N}.tmp");

        try
        {
            var document = new BingoGameDocument
            {
                Version = CurrentVersion,
                DrawnNumbers = game.DrawnNumbers.ToArray()
            };
            await using (var stream = new FileStream(
                temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(
                    stream, document, PersistenceJsonContext.Default.BingoGameDocument, cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            try
            {
                File.Delete(temporaryPath);
            }
            catch (IOException exception)
            {
                _logger.LogWarning(exception, "抽選履歴の一時ファイルを削除できませんでした: {Path}", temporaryPath);
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogWarning(exception, "抽選履歴の一時ファイルを削除できませんでした: {Path}", temporaryPath);
            }
        }
    }
}
