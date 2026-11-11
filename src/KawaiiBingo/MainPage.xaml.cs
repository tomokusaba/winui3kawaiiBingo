using KawaiiBingo.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;
using Windows.UI;

namespace KawaiiBingo;

/// <summary>抽選とビンゴ盤を表示するメイン画面です。</summary>
public sealed partial class MainPage : Page
{
    /// <summary>一度に飛ばす紙ふぶきの数です。</summary>
    private const int ConfettiCount = 22;

    /// <summary>紙ふぶきに使う形です。</summary>
    private static readonly string[] ConfettiShapes = ["♥", "✦", "✿", "★", "♡", "●"];

    /// <summary>紙ふぶきに使うパステルカラーです。</summary>
    private static readonly Color[] ConfettiColors =
    [
        Color.FromArgb(255, 0xF5, 0x78, 0xA6), Color.FromArgb(255, 0xF3, 0xB4, 0x4E),
        Color.FromArgb(255, 0x68, 0xCA, 0xA2), Color.FromArgb(255, 0xA8, 0x8B, 0xEE),
        Color.FromArgb(255, 0xFF, 0x9E, 0xC0), Color.FromArgb(255, 0x7C, 0xC6, 0xF0)
    ];

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
        ViewModel.EffectRequested += OnEffectRequested;
    }

    /// <summary>画面の表示時に保存済みの状態を読み込みます。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="args">イベントの情報です。</param>
    private async void OnLoaded(object sender, RoutedEventArgs args) => await ViewModel.InitializeAsync();

    /// <summary>新しい番号が決まったときに読み上げを行います。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="args">イベントの情報です。</param>
    private void OnRevealed(object? sender, EventArgs args)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(CurrentBallText);
        peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }

    /// <summary>抽選演出の各場面に合わせてアニメーションを再生します。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="effect">再生する効果です。</param>
    private void OnEffectRequested(object? sender, DrawEffect effect)
    {
        switch (effect)
        {
            case DrawEffect.Tick:
                TickStoryboard.Begin();
                break;
            case DrawEffect.FakeStop:
                FakeStopStoryboard.Begin();
                break;
            case DrawEffect.SneakStart:
                SneakStoryboard.Begin();
                break;
            case DrawEffect.Nudge:
                NudgeStoryboard.Begin();
                break;
            case DrawEffect.Reveal:
                RevealStoryboard.Begin();
                BurstConfetti();
                break;
            case DrawEffect.Cancel:
                FakeStopStoryboard.Stop();
                SneakStoryboard.Stop();
                NudgeStoryboard.Stop();
                break;
        }
    }

    /// <summary>ボールの中心からハートや星の紙ふぶきを飛ばします。</summary>
    private void BurstConfetti()
    {
        ConfettiLayer.Children.Clear();
        var storyboard = new Storyboard();
        var center = ConfettiLayer.Width / 2;
        for (var index = 0; index < ConfettiCount; index++)
        {
            var angle = Math.PI * 2 * index / ConfettiCount + Random.Shared.NextDouble() * 0.4;
            var distance = 120 + Random.Shared.NextDouble() * 70;
            var transform = new CompositeTransform();
            var piece = new TextBlock
            {
                Text = ConfettiShapes[index % ConfettiShapes.Length],
                FontSize = 16 + Random.Shared.Next(14),
                Foreground = new SolidColorBrush(ConfettiColors[index % ConfettiColors.Length]),
                RenderTransform = transform,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            Canvas.SetLeft(piece, center - 10);
            Canvas.SetTop(piece, center - 14);
            ConfettiLayer.Children.Add(piece);

            var duration = TimeSpan.FromMilliseconds(900 + Random.Shared.Next(500));
            AddAnimation(storyboard, transform, "TranslateX", 0, Math.Cos(angle) * distance, duration, new CubicEase { EasingMode = EasingMode.EaseOut });
            AddAnimation(storyboard, transform, "TranslateY", 0, Math.Sin(angle) * distance + 40, duration, new QuadraticEase { EasingMode = EasingMode.EaseOut });
            AddAnimation(storyboard, transform, "Rotation", 0, Random.Shared.Next(-240, 240), duration, null);
            AddAnimation(storyboard, transform, "ScaleX", 0.3, 1.1, TimeSpan.FromMilliseconds(260), new BackEase { EasingMode = EasingMode.EaseOut });
            AddAnimation(storyboard, transform, "ScaleY", 0.3, 1.1, TimeSpan.FromMilliseconds(260), new BackEase { EasingMode = EasingMode.EaseOut });
            var fade = new DoubleAnimation
            {
                From = 1,
                To = 0,
                BeginTime = duration * 0.55,
                Duration = duration * 0.45
            };
            Storyboard.SetTarget(fade, piece);
            Storyboard.SetTargetProperty(fade, "Opacity");
            storyboard.Children.Add(fade);
        }

        storyboard.Completed += (_, _) => ConfettiLayer.Children.Clear();
        storyboard.Begin();
    }

    /// <summary>紙ふぶき用のアニメーションを一つ追加します。</summary>
    /// <param name="storyboard">追加先のストーリーボードです。</param>
    /// <param name="target">動かす変形です。</param>
    /// <param name="property">動かすプロパティ名です。</param>
    /// <param name="from">開始値です。</param>
    /// <param name="to">終了値です。</param>
    /// <param name="duration">再生時間です。</param>
    /// <param name="easing">緩急の付け方、または等速の場合の null です。</param>
    private static void AddAnimation(
        Storyboard storyboard,
        DependencyObject target,
        string property,
        double from,
        double to,
        TimeSpan duration,
        EasingFunctionBase? easing)
    {
        var animation = new DoubleAnimation { From = from, To = to, Duration = duration, EasingFunction = easing };
        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, property);
        storyboard.Children.Add(animation);
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
