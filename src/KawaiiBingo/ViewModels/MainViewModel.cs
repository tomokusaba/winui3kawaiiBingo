using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KawaiiBingo.UseCases.Games;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace KawaiiBingo.ViewModels;

/// <summary>ビンゴ画面の状態と操作を管理します。</summary>
public sealed class MainViewModel : ObservableObject
{
    /// <summary>抽選操作のユースケースです。</summary>
    private readonly BingoGameService _gameService;

    /// <summary>操作失敗時のログ記録先です。</summary>
    private readonly ILogger<MainViewModel> _logger;

    /// <summary>最後に正常に読み込んだ抽選状態です。</summary>
    private BingoSnapshot? _snapshot;

    /// <summary>ボールに表示している番号です。</summary>
    private int? _displayNumber;

    /// <summary>読み込みまたは操作中かどうかです。</summary>
    private bool _isBusy;

    /// <summary>読み込みが完了したかどうかです。</summary>
    private bool _isReady;

    /// <summary>抽選演出中にボタンへ表示する文字列です。演出中でない場合は null です。</summary>
    private string? _drawingButtonText;

    /// <summary>抽選演出でスポットライトが当たっているセルです。</summary>
    private BingoCellViewModel? _spotlightCell;

    /// <summary>マスコットの案内文です。</summary>
    private string _message = "いっしょに あそぼう！";

    /// <summary>画面に表示するエラーです。</summary>
    private string _errorMessage = string.Empty;

    /// <summary>画面の状態と操作を初期化します。</summary>
    /// <param name="gameService">抽選のユースケースです。</param>
    /// <param name="logger">ログの記録先です。</param>
    public MainViewModel(BingoGameService gameService, ILogger<MainViewModel> logger)
    {
        _gameService = gameService;
        _logger = logger;
        Rows = Enumerable.Range(0, 5).Select(index => new BingoRowViewModel(index * 15 + 1)).ToArray();
        DrawCommand = new AsyncRelayCommand(DrawAsync, CanDrawNumber);
        UndoCommand = new AsyncRelayCommand(UndoAsync, CanUndoNumber);
        RetryCommand = new AsyncRelayCommand(InitializeAsync, CanRetry);
    }

    /// <summary>新しい番号が決まったときに発生します。</summary>
    public event EventHandler? Revealed;

    /// <summary>抽選演出の効果を画面で再生してほしいときに発生します。</summary>
    public event EventHandler<DrawEffect>? EffectRequested;

    /// <summary>ビンゴ盤の五行を取得します。</summary>
    public IReadOnlyList<BingoRowViewModel> Rows { get; }

    /// <summary>現在の番号より前に抽選された最近の番号を取得します。</summary>
    public ObservableCollection<RecentNumberViewModel> RecentNumbers { get; } = [];

    /// <summary>番号を抽選するコマンドを取得します。</summary>
    public IAsyncRelayCommand DrawCommand { get; }

    /// <summary>最後の抽選を取り消すコマンドを取得します。</summary>
    public IAsyncRelayCommand UndoCommand { get; }

    /// <summary>状態の読み込みを再試行するコマンドを取得します。</summary>
    public IAsyncRelayCommand RetryCommand { get; }

    /// <summary>ボールに表示する番号を取得します。</summary>
    public string CurrentNumberText => _displayNumber?.ToString(CultureInfo.InvariantCulture) ?? "?";

    /// <summary>ボールに表示するアルファベットを取得します。</summary>
    public string CurrentLetter => _displayNumber is int number ? BingoPalette.Letter(number) : "♡";

    /// <summary>ボールの色を取得します。</summary>
    public Brush CurrentBallBrush => BingoPalette.Ball(_displayNumber ?? 1);

    /// <summary>支援技術向けに現在の番号を取得します。</summary>
    public string CurrentNumberDescription => _displayNumber is int number
        ? $"現在の番号は {BingoPalette.Letter(number)} の {number} です"
        : "まだ番号は抽選されていません";

    /// <summary>抽選済みの個数を表示する文字列を取得します。</summary>
    public string CountText => $"{_snapshot?.DrawnCount ?? 0} / 75";

    /// <summary>残りの個数を表示する文字列を取得します。</summary>
    public string RemainingText => $"のこり {_snapshot?.RemainingCount ?? 75} こ";

    /// <summary>進捗バーに表示する抽選済みの個数を取得します。</summary>
    public double ProgressValue => _snapshot?.DrawnCount ?? 0;

    /// <summary>抽選ボタンのラベルを取得します。</summary>
    public string DrawButtonText => _drawingButtonText
        ?? (_snapshot?.IsComplete == true ? "ぜんぶ でたよ！" : "くじを ひく！");

    /// <summary>マスコットの案内文を取得します。</summary>
    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    /// <summary>画面に表示するエラーを取得します。</summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(ErrorVisibility));
            }
        }
    }

    /// <summary>エラー表示の可視状態を取得します。</summary>
    public Visibility ErrorVisibility => string.IsNullOrEmpty(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>読み込み再試行ボタンの可視状態を取得します。</summary>
    public Visibility RetryVisibility => !_isReady && !string.IsNullOrEmpty(ErrorMessage)
        ? Visibility.Visible
        : Visibility.Collapsed;

    /// <summary>最近の番号がないときの案内の可視状態を取得します。</summary>
    public Visibility EmptyRecentVisibility => RecentNumbers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>抽選履歴をリセットできるかどうかを取得します。</summary>
    public bool CanReset => _isReady && !_isBusy && _snapshot?.DrawnCount > 0;

    /// <summary>保存された抽選状態を読み込みます。</summary>
    /// <returns>読み込みの完了を表すタスクです。</returns>
    public async Task InitializeAsync()
    {
        if (_isReady || _isBusy)
        {
            return;
        }

        SetBusy(true);
        ErrorMessage = string.Empty;
        try
        {
            var snapshot = await _gameService.GetAsync();
            ApplySnapshot(snapshot);
            _isReady = true;
            Message = snapshot.IsComplete
                ? "ぜんぶ でたよ！ おめでとう！"
                : snapshot.DrawnCount == 0 ? "いっしょに あそぼう！" : "つづきから あそぼう！";
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>確認後に抽選履歴をリセットします。</summary>
    /// <returns>リセットの完了を表すタスクです。</returns>
    public async Task ResetAsync()
    {
        if (!CanReset)
        {
            return;
        }

        SetBusy(true);
        ErrorMessage = string.Empty;
        try
        {
            ApplySnapshot(await _gameService.ResetAsync());
            Message = "また いっしょに あそぼう！";
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>抽選演出のあとに番号を一つ抽選します。</summary>
    /// <returns>抽選の完了を表すタスクです。</returns>
    private async Task DrawAsync()
    {
        if (!CanDrawNumber() || _snapshot is null)
        {
            return;
        }

        SetBusy(true);
        SetDrawButtonText("まぜまぜ…");
        ErrorMessage = string.Empty;
        Message = "まぜまぜ まぜまぜ…♪";
        var previous = _snapshot;
        try
        {
            var candidates = previous.RemainingNumbers;
            var snapshot = await _gameService.DrawAsync();
            var winner = snapshot.LastNumber ?? throw new InvalidOperationException("抽選結果の番号がありません。");

            await PlayRiggedShowAsync(candidates, winner);

            SetSpotlight(null);
            ApplySnapshot(snapshot);
            Message = snapshot.IsComplete
                ? "ぜんぶ でたよ！ おめでとう！"
                : $"{CurrentLetter} の {CurrentNumberText} が でたよ！ …ぐうぜん♡";
            RaiseEffect(DrawEffect.Reveal);
            Revealed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            SetSpotlight(null);
            ApplySnapshot(previous);
            RaiseEffect(DrawEffect.Cancel);
            ReportError(exception);
        }
        finally
        {
            SetDrawButtonText(null);
            SetBusy(false);
        }
    }

    /// <summary>ルーレット、おとりでの停止、こっそりずらす動きを順番に再生します。</summary>
    /// <param name="candidates">抽選前に残っていた番号です。</param>
    /// <param name="winner">実際に抽選された番号です。</param>
    /// <returns>演出の完了を表すタスクです。</returns>
    private async Task PlayRiggedShowAsync(IReadOnlyList<int> candidates, int winner)
    {
        var script = RiggedShowScript.Create(candidates, winner, Random.Shared);
        if (script.Decoy is not int decoy)
        {
            Message = "さいごの ひとつ…！";
            await SpinAsync(candidates, 10, winner);
            return;
        }

        await SpinAsync(candidates, 22, decoy);
        RaiseEffect(DrawEffect.FakeStop);
        Message = $"{BingoPalette.Letter(decoy)} の {decoy} に けってい…？";
        SetDrawButtonText("けってい…？");
        await Task.Delay(1100);

        RaiseEffect(DrawEffect.SneakStart);
        Message = "…あっ、ちょっと まってね。";
        SetDrawButtonText("ないしょ…");
        await Task.Delay(750);

        Message = "ずりずり… ずりずり…";
        for (var index = 0; index < script.NudgePath.Count; index++)
        {
            var number = script.NudgePath[index];
            SetDisplayNumber(number);
            SetSpotlight(number);
            RaiseEffect(DrawEffect.Nudge);
            var isLast = index == script.NudgePath.Count - 1;
            await Task.Delay(isLast ? 520 : 330);
        }
    }

    /// <summary>ボールとボードのスポットライトを、だんだん遅くしながら回します。</summary>
    /// <param name="candidates">表示に使う番号です。</param>
    /// <param name="steps">回す回数です。</param>
    /// <param name="stopAt">最後に止まって見せる番号です。</param>
    /// <returns>回転の完了を表すタスクです。</returns>
    private async Task SpinAsync(IReadOnlyList<int> candidates, int steps, int stopAt)
    {
        for (var step = 0; step < steps; step++)
        {
            var number = step == steps - 1 ? stopAt : candidates[Random.Shared.Next(candidates.Count)];
            SetDisplayNumber(number);
            SetSpotlight(number);
            RaiseEffect(DrawEffect.Tick);
            var progress = (double)step / steps;
            await Task.Delay((int)(45 + 260 * progress * progress * progress));
        }
    }

    /// <summary>指定した番号のセルだけにスポットライトを当てます。</summary>
    /// <param name="number">スポットライトを当てる番号、または消す場合の null です。</param>
    private void SetSpotlight(int? number)
    {
        if (_spotlightCell is not null)
        {
            _spotlightCell.SetSpotlight(false);
            _spotlightCell = null;
        }

        if (number is int value)
        {
            _spotlightCell = Rows[(value - 1) / 15].Cells[(value - 1) % 15];
            _spotlightCell.SetSpotlight(true);
        }
    }

    /// <summary>画面に演出効果の再生を依頼します。</summary>
    /// <param name="effect">再生する効果です。</param>
    private void RaiseEffect(DrawEffect effect) => EffectRequested?.Invoke(this, effect);

    /// <summary>抽選中のボタン表示を切り替えます。</summary>
    /// <param name="text">抽選中に表示する文字列、または通常表示に戻す場合の null です。</param>
    private void SetDrawButtonText(string? text)
    {
        _drawingButtonText = text;
        OnPropertyChanged(nameof(DrawButtonText));
    }

    /// <summary>最後の抽選を取り消します。</summary>
    /// <returns>取り消しの完了を表すタスクです。</returns>
    private async Task UndoAsync()
    {
        if (!CanUndoNumber())
        {
            return;
        }

        SetBusy(true);
        ErrorMessage = string.Empty;
        try
        {
            ApplySnapshot(await _gameService.UndoAsync());
            Message = "ひとつ もどしたよ！";
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>抽選状態を画面に反映します。</summary>
    /// <param name="snapshot">表示する抽選状態です。</param>
    private void ApplySnapshot(BingoSnapshot snapshot)
    {
        _snapshot = snapshot;
        SetDisplayNumber(snapshot.LastNumber);
        foreach (var row in Rows)
        {
            foreach (var cell in row.Cells)
            {
                cell.SetState(snapshot.IsDrawn(cell.Number), snapshot.LastNumber == cell.Number);
            }
        }

        RecentNumbers.Clear();
        foreach (var number in snapshot.DrawnNumbers.Reverse().Skip(1).Take(6))
        {
            RecentNumbers.Add(new RecentNumberViewModel(number));
        }

        OnPropertyChanged(nameof(CountText));
        OnPropertyChanged(nameof(RemainingText));
        OnPropertyChanged(nameof(ProgressValue));
        OnPropertyChanged(nameof(EmptyRecentVisibility));
        OnPropertyChanged(nameof(DrawButtonText));
    }

    /// <summary>ボールに表示する番号を更新します。</summary>
    /// <param name="number">表示する番号、または未抽選を表す null です。</param>
    private void SetDisplayNumber(int? number)
    {
        if (_displayNumber == number)
        {
            return;
        }

        _displayNumber = number;
        OnPropertyChanged(nameof(CurrentNumberText));
        OnPropertyChanged(nameof(CurrentLetter));
        OnPropertyChanged(nameof(CurrentBallBrush));
        OnPropertyChanged(nameof(CurrentNumberDescription));
    }

    /// <summary>操作中の状態と各ボタンの有効状態を更新します。</summary>
    /// <param name="isBusy">操作中の場合は真です。</param>
    private void SetBusy(bool isBusy)
    {
        _isBusy = isBusy;
        OnPropertyChanged(nameof(DrawButtonText));
        OnPropertyChanged(nameof(CanReset));
        OnPropertyChanged(nameof(RetryVisibility));
        DrawCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        RetryCommand.NotifyCanExecuteChanged();
    }

    /// <summary>抽選できるかどうかを返します。</summary>
    /// <returns>抽選できる場合は真です。</returns>
    private bool CanDrawNumber() => _isReady && !_isBusy && _snapshot?.IsComplete == false;

    /// <summary>最後の抽選を取り消せるかどうかを返します。</summary>
    /// <returns>取り消せる場合は真です。</returns>
    private bool CanUndoNumber() => _isReady && !_isBusy && _snapshot?.DrawnCount > 0;

    /// <summary>初期読み込みを再試行できるかどうかを返します。</summary>
    /// <returns>再試行できる場合は真です。</returns>
    private bool CanRetry() => !_isReady && !_isBusy;

    /// <summary>操作の失敗を記録して画面に通知します。</summary>
    /// <param name="exception">発生した例外です。</param>
    private void ReportError(Exception exception)
    {
        _logger.LogError(exception, "ビンゴの抽選状態を操作できませんでした。");
        Message = "ごめんね、もういちど おねがい！";
        ErrorMessage = $"抽選データを読み書きできませんでした。保存先を確認して、もう一度お試しください。 ({exception.Message})";
        OnPropertyChanged(nameof(RetryVisibility));
    }
}
