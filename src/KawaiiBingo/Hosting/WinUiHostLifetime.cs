using Microsoft.Extensions.Hosting;

namespace KawaiiBingo.Hosting;

/// <summary>WinUI のメッセージループにホストの生存期間を委ねます。</summary>
internal sealed class WinUiHostLifetime : IHostLifetime
{
    /// <summary>ホストの起動を待機します。</summary>
    /// <param name="cancellationToken">待機のキャンセルを通知します。</param>
    /// <returns>完了済みのタスクです。</returns>
    public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>ホストの停止を待機します。</summary>
    /// <param name="cancellationToken">待機のキャンセルを通知します。</param>
    /// <returns>完了済みのタスクです。</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
