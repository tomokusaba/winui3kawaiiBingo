using KawaiiBingo.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace KawaiiBingo;

/// <summary>抽選とビンゴ盤を表示するメイン画面です。</summary>
public sealed partial class MainPage : Page
{
    /// <summary>リセットの確認画面が表示中かどうかです。</summary>
    private bool _isResetDialogOpen;

    /// <summary>画面に対応する状態を取得します。</summary>
    public MainViewModel ViewModel { get; }

    /// <summary>画面を作成します。</summary>
    /// <param name="viewModel">画面の状態と操作です。</param>
    public MainPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
        ViewModel.Revealed += OnRevealed;
    }

    /// <summary>画面の表示時に保存済みの状態を読み込みます。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="args">イベントの情報です。</param>
    private async void OnLoaded(object sender, RoutedEventArgs args) => await ViewModel.InitializeAsync();

    /// <summary>新しい番号が決まったときに演出と読み上げを行います。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="args">イベントの情報です。</param>
    private void OnRevealed(object? sender, EventArgs args)
    {
        RevealStoryboard.Begin();
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(CurrentBallText);
        peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }

    /// <summary>確認してから抽選履歴をリセットします。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="args">イベントの情報です。</param>
    private async void OnResetClick(object sender, RoutedEventArgs args)
    {
        if (_isResetDialogOpen || !ViewModel.CanReset)
        {
            return;
        }

        _isResetDialogOpen = true;
        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "はじめから あそぶ？",
                Content = "これまでに でた ばんごうを ぜんぶ けすよ。",
                PrimaryButtonText = "はじめから",
                SecondaryButtonText = "やめておく",
                DefaultButton = ContentDialogButton.Secondary
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await ViewModel.ResetAsync();
            }
        }
        finally
        {
            _isResetDialogOpen = false;
        }
    }
}
