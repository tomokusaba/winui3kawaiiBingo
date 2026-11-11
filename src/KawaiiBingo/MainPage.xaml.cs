using KawaiiBingo.Layout;
using KawaiiBingo.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
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
    /// <summary>通常の演出で一度に飛ばす紙ふぶきの数です。</summary>
    private const int ConfettiCount = 22;

    /// <summary>紙ふぶきに使うパステルカラーです。</summary>
    private static readonly Color[] ConfettiColors =
    [
        Color.FromArgb(255, 0xF5, 0x78, 0xA6), Color.FromArgb(255, 0xF3, 0xB4, 0x4E),
        Color.FromArgb(255, 0x68, 0xCA, 0xA2), Color.FromArgb(255, 0xA8, 0x8B, 0xEE),
        Color.FromArgb(255, 0xFF, 0x9E, 0xC0), Color.FromArgb(255, 0x7C, 0xC6, 0xF0)
    ];

    /// <summary>リセットの確認画面が表示中かどうかです。</summary>
    private bool _isResetDialogOpen;

    /// <summary>現在再生中のいたずら演出です。</summary>
    private Storyboard? _trickStoryboard;

    /// <summary>現在再生中の紙ふぶきです。</summary>
    private Storyboard? _confettiStoryboard;

    /// <summary>今回選ばれた演出の効果です。</summary>
    private DrawEffect? _currentTrick;

    /// <summary>画面に対応する状態を取得します。</summary>
    public MainViewModel ViewModel { get; }

    /// <summary>全画面表示の切り替えが要求されたときに発生します。</summary>
    public event EventHandler? FullScreenRequested;

    /// <summary>画面を作成します。</summary>
    /// <param name="viewModel">画面の状態と操作です。</param>
    public MainPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        InitializeNumberBoard();
        Loaded += OnLoaded;
        ViewModel.Revealed += OnRevealed;
        ViewModel.EffectRequested += OnEffectRequested;
    }

    /// <summary>画面の表示時に保存済みの状態を読み込みます。</summary>
    /// <param name="sender">イベントの送信元です。</param>
    /// <param name="args">イベントの情報です。</param>
    private async void OnLoaded(object sender, RoutedEventArgs args) => await ViewModel.InitializeAsync();

    /// <summary>番号と見出しを、表示領域全体に広がる均等なマスへ配置します。</summary>
    private void InitializeNumberBoard()
    {
        var letterTemplate = (DataTemplate)Resources["BoardLetterTemplate"];
        var numberTemplate = (DataTemplate)Resources["BoardNumberTemplate"];
        var columnCount = ViewModel.Rows[0].Cells.Count + 1;
        for (var column = 0; column < columnCount; column++)
        {
            NumberBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (var row = 0; row < ViewModel.Rows.Count; row++)
        {
            NumberBoard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddBoardItem(ViewModel.Rows[row], letterTemplate, row, 0);
            for (var column = 0; column < ViewModel.Rows[row].Cells.Count; column++)
            {
                AddBoardItem(ViewModel.Rows[row].Cells[column], numberTemplate, row, column + 1);
            }
        }
    }

    /// <summary>指定されたマスに、見出しまたは番号の表示を追加します。</summary>
    /// <param name="item">表示する画面モデルです。</param>
    /// <param name="template">表示に使うテンプレートです。</param>
    /// <param name="row">表示する行です。</param>
    /// <param name="column">表示する列です。</param>
    private void AddBoardItem(object item, DataTemplate template, int row, int column)
    {
        var presenter = new ContentPresenter
        {
            Content = item,
            ContentTemplate = template,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        Grid.SetRow(presenter, row);
        Grid.SetColumn(presenter, column);
        NumberBoard.Children.Add(presenter);
    }

    /// <summary>ウィンドウの変更に合わせて画面全体の拡大率と配置を更新します。</summary>
    /// <param name="sender">表示領域です。</param>
    /// <param name="args">変更後のサイズです。</param>
    private void OnViewportSizeChanged(object sender, SizeChangedEventArgs args) => UpdateResponsiveLayout();

    /// <summary>エラーが表示された場合も操作部の領域を確保します。</summary>
    /// <param name="sender">エラー表示です。</param>
    /// <param name="args">変更後のサイズです。</param>
    private void OnErrorPanelSizeChanged(object sender, SizeChangedEventArgs args) => UpdateResponsiveLayout();

    /// <summary>余白だけを増やさず、文字や演出を同じ倍率で拡大して表示領域を使います。</summary>
    private void UpdateResponsiveLayout()
    {
        if (LayoutViewport.ActualWidth <= 0 || LayoutViewport.ActualHeight <= 0)
        {
            return;
        }

        var errorHeight = ErrorPanel.Visibility == Visibility.Visible ? ErrorPanel.ActualHeight + 14 : 0;
        var layout = BingoLayout.Fit(LayoutViewport.ActualWidth, LayoutViewport.ActualHeight, errorHeight);
        ContentSurface.Width = layout.Width;
        ContentSurface.Height = layout.Height;
        StageColumn.Width = new GridLength(1, GridUnitType.Star);
        BoardColumn.Width = layout.IsWide ? new GridLength(2.15, GridUnitType.Star) : new GridLength(0);
        StageRow.Height = new GridLength(layout.IsWide ? 1 : 1.1, GridUnitType.Star);
        BoardRow.Height = layout.IsWide ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        MainContent.ColumnSpacing = layout.IsWide ? 20 : 0;
        MainContent.RowSpacing = layout.IsWide ? 0 : 16;
        Grid.SetRow(BoardPanel, layout.IsWide ? 0 : 1);
        Grid.SetColumn(BoardPanel, layout.IsWide ? 1 : 0);
    }

    /// <summary>全画面表示の切り替えをウィンドウに要求します。</summary>
    /// <param name="sender">全画面ボタンです。</param>
    /// <param name="args">クリックの情報です。</param>
    private void OnFullScreenClick(object sender, RoutedEventArgs args) => FullScreenRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>全画面ボタンの案内を現在の表示状態に合わせます。</summary>
    /// <param name="isFullScreen">全画面表示中かどうかです。</param>
    public void SetFullScreenState(bool isFullScreen)
    {
        FullScreenLabel.Text = isFullScreen ? "全画面を終了" : "全画面";
        FullScreenIcon.Glyph = isFullScreen ? "\uE73F" : "\uE740";
        AutomationProperties.SetName(FullScreenButton, FullScreenLabel.Text);
    }

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
            case DrawEffect.Begin:
                ResetEffects();
                _currentTrick = null;
                break;
            case DrawEffect.Tick:
                TickStoryboard.Begin();
                break;
            case DrawEffect.FakeStop:
                TickStoryboard.Stop();
                FakeStopStoryboard.Begin();
                break;
            case DrawEffect.SneakStart:
                StartTrick(effect);
                SneakStoryboard.Begin();
                break;
            case DrawEffect.Nudge:
                NudgeStoryboard.Begin();
                break;
            case DrawEffect.Magic:
            case DrawEffect.Magnet:
            case DrawEffect.Gift:
            case DrawEffect.Stamp:
            case DrawEffect.Ufo:
            case DrawEffect.Cloud:
            case DrawEffect.Reverse:
            case DrawEffect.Carrot:
            case DrawEffect.Rainbow:
                StartTrick(effect);
                break;
            case DrawEffect.Reveal:
                var trick = _currentTrick;
                ResetEffects();
                RevealStoryboard.Begin();
                BurstConfetti(trick);
                break;
            case DrawEffect.Cancel:
                ResetEffects();
                _currentTrick = null;
                break;
        }
    }

    /// <summary>前の演出を停止して、一時的な表示を取り除きます。</summary>
    private void ResetEffects()
    {
        TickStoryboard.Stop();
        FakeStopStoryboard.Stop();
        SneakStoryboard.Stop();
        NudgeStoryboard.Stop();
        RevealStoryboard.Stop();
        _trickStoryboard?.Stop();
        _trickStoryboard = null;
        _confettiStoryboard?.Stop();
        _confettiStoryboard = null;
        ConfettiLayer.Children.Clear();
    }

    /// <summary>選ばれたいたずらに合わせて小道具とボールを動かします。</summary>
    /// <param name="effect">再生するいたずらの効果です。</param>
    private void StartTrick(DrawEffect effect)
    {
        TickStoryboard.Stop();
        FakeStopStoryboard.Stop();
        _trickStoryboard?.Stop();
        _currentTrick = effect;

        var (symbol, label, color, size) = effect switch
        {
            DrawEffect.SneakStart => ("", "にくきゅう", Color.FromArgb(255, 0xEF, 0x9E, 0xBB), 76d),
            DrawEffect.Magic => ("🪄", "まほうのつえ", Color.FromArgb(255, 0xB1, 0x89, 0xE5), 76d),
            DrawEffect.Magnet => ("🧲", "くっつきじしゃく", Color.FromArgb(255, 0x66, 0xB7, 0xD6), 82d),
            DrawEffect.Gift => ("🎁", "ひみつのはこ", Color.FromArgb(255, 0xEF, 0x8B, 0xB1), 104d),
            DrawEffect.Stamp => ("💮", "こっそり はんこ", Color.FromArgb(255, 0xEC, 0x84, 0x98), 104d),
            DrawEffect.Ufo => ("🛸", "うちゅうから！？", Color.FromArgb(255, 0x6C, 0xC9, 0xBD), 82d),
            DrawEffect.Cloud => ("☁️", "くもの かくれんぼ", Color.FromArgb(255, 0x89, 0xB8, 0xDB), 150d),
            DrawEffect.Reverse => ("↺", "ぎゃくかいてん", Color.FromArgb(255, 0xE7, 0xAD, 0x48), 110d),
            DrawEffect.Carrot => ("🥕", "にんじんで よそみ", Color.FromArgb(255, 0xEC, 0xA1, 0x5E), 100d),
            DrawEffect.Rainbow => ("🌈", "レインボー！", Color.FromArgb(255, 0xDF, 0x9D, 0xCE), 110d),
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, "未知の演出です。")
        };

        TrickSymbol.Text = symbol;
        TrickSymbol.FontSize = size;
        TrickSymbol.Foreground = new SolidColorBrush(color);
        TrickBadgeText.Text = label;
        TrickBadge.BorderBrush = new SolidColorBrush(color);
        TrickRing.Stroke = new SolidColorBrush(color);

        var storyboard = new Storyboard();
        _trickStoryboard = storyboard;
        AddKeyFrames(storyboard, TrickBadge, "Opacity", (0, 0), (160, 1));
        AddKeyFrames(storyboard, TrickBadgeScale, "ScaleX", (0, 0.6), (240, 1));
        AddKeyFrames(storyboard, TrickBadgeScale, "ScaleY", (0, 0.6), (240, 1));
        AddKeyFrames(storyboard, TrickRing, "Opacity", (0, 0), (180, 0.8), (950, 0.45), (1300, 0));
        AddKeyFrames(storyboard, TrickRingScale, "ScaleX", (0, 0.8), (650, 1.07), (1300, 1.15));
        AddKeyFrames(storyboard, TrickRingScale, "ScaleY", (0, 0.8), (650, 1.07), (1300, 1.15));

        if (effect != DrawEffect.SneakStart)
        {
            var fadeStart = effect == DrawEffect.Rainbow ? 1450 : 900;
            AddKeyFrames(storyboard, TrickSymbol, "Opacity", (0, 0), (120, 1), (fadeStart, 1), (fadeStart + 250, 0));
        }

        switch (effect)
        {
            case DrawEffect.Magic:
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateX", (0, -110), (500, 60), (1000, 90));
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateY", (0, -65), (500, 40), (1000, -75));
                AddKeyFrames(storyboard, TrickSymbolTransform, "Rotation", (0, -60), (500, 65), (1000, -20));
                AddKeyFrames(storyboard, BallTransform, "ScaleX", (0, 1), (400, 0.08), (550, 0.08), (1050, 1));
                AddKeyFrames(storyboard, BallTransform, "ScaleY", (0, 1), (400, 0.08), (550, 0.08), (1050, 1));
                break;
            case DrawEffect.Magnet:
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateX", (0, 135), (450, 70), (1050, 115));
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateY", (0, -70), (450, -45), (1050, -70));
                AddKeyFrames(storyboard, TrickSymbolTransform, "Rotation", (0, -25), (450, 25), (1050, -15));
                AddKeyFrames(storyboard, BallTransform, "TranslateX", (0, 0), (470, 32), (700, -14), (1100, 0));
                AddKeyFrames(storyboard, BallTransform, "Rotation", (0, 0), (470, 10), (700, -8), (1100, 0));
                break;
            case DrawEffect.Gift:
                AddKeyFrames(storyboard, TrickSymbolTransform, "ScaleX", (0, 0.2), (350, 1.65), (680, 1.65), (1150, 0.05));
                AddKeyFrames(storyboard, TrickSymbolTransform, "ScaleY", (0, 0.2), (350, 1.65), (680, 1.65), (1150, 0.05));
                AddKeyFrames(storyboard, TrickSymbolTransform, "Rotation", (0, -12), (350, 8), (680, -7), (1150, 0));
                AddKeyFrames(storyboard, BallTransform, "ScaleY", (0, 1), (400, 0.82), (1050, 1));
                break;
            case DrawEffect.Stamp:
                AddKeyFrames(storyboard, TrickSymbolTransform, "ScaleX", (0, 2.3), (280, 1.1), (650, 1.15), (1050, 0.1));
                AddKeyFrames(storyboard, TrickSymbolTransform, "ScaleY", (0, 2.3), (280, 1.1), (650, 1.15), (1050, 0.1));
                AddKeyFrames(storyboard, TrickSymbolTransform, "Rotation", (0, -20), (280, -12), (1050, 15));
                AddKeyFrames(storyboard, BallTransform, "ScaleX", (0, 1), (300, 1.16), (650, 1));
                AddKeyFrames(storyboard, BallTransform, "ScaleY", (0, 1), (300, 0.78), (650, 1));
                break;
            case DrawEffect.Ufo:
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateY", (0, -150), (300, -92), (900, -92), (1300, -150));
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateX", (0, 35), (300, 0), (900, 0), (1300, -45));
                AddKeyFrames(storyboard, TrickBeam, "Opacity", (0, 0), (300, 0.5), (850, 0.5), (1200, 0));
                AddKeyFrames(storyboard, BallTransform, "TranslateY", (0, 0), (550, -45), (1200, 0));
                AddKeyFrames(storyboard, BallTransform, "ScaleX", (0, 1), (550, 0.6), (1200, 1));
                AddKeyFrames(storyboard, BallTransform, "ScaleY", (0, 1), (550, 0.6), (1200, 1));
                break;
            case DrawEffect.Cloud:
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateX", (0, -145), (450, 0), (700, 0), (1200, 145));
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateY", (0, 15), (450, -5), (1200, 15));
                AddKeyFrames(storyboard, BallTransform, "ScaleX", (0, 1), (500, 0.88), (1100, 1));
                AddKeyFrames(storyboard, BallTransform, "ScaleY", (0, 1), (500, 0.88), (1100, 1));
                break;
            case DrawEffect.Reverse:
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateX", (0, 80), (1300, 80));
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateY", (0, -80), (1300, -80));
                AddKeyFrames(storyboard, TrickSymbolTransform, "Rotation", (0, 0), (1300, -360));
                AddKeyFrames(storyboard, BallTransform, "Rotation", (0, 0), (1300, -720));
                break;
            case DrawEffect.Carrot:
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateX", (0, 135), (600, -30), (1150, -135));
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateY", (0, -30), (600, 5), (1150, -45));
                AddKeyFrames(storyboard, TrickSymbolTransform, "Rotation", (0, 45), (600, -35), (1150, 25));
                AddKeyFrames(storyboard, BallTransform, "TranslateX", (0, 0), (350, 15), (750, -18), (1100, 0));
                AddKeyFrames(storyboard, BallTransform, "Rotation", (0, 0), (350, 8), (750, -10), (1100, 0));
                break;
            case DrawEffect.Rainbow:
                AddKeyFrames(storyboard, TrickSymbolTransform, "TranslateY", (0, -110), (400, -70), (1450, -70));
                AddKeyFrames(storyboard, TrickSymbolTransform, "ScaleX", (0, 0.4), (400, 1.1), (1000, 0.9), (1450, 1.1));
                AddKeyFrames(storyboard, TrickSymbolTransform, "ScaleY", (0, 0.4), (400, 1.1), (1000, 0.9), (1450, 1.1));
                AddKeyFrames(storyboard, BallTransform, "Rotation", (0, 0), (350, -12), (700, 12), (1050, -8), (1450, 0));
                AddKeyFrames(storyboard, BallTransform, "ScaleX", (0, 1), (400, 1.1), (800, 0.94), (1200, 1.1), (1650, 1));
                AddKeyFrames(storyboard, BallTransform, "ScaleY", (0, 1), (400, 1.1), (800, 0.94), (1200, 1.1), (1650, 1));
                BurstConfetti(effect);
                break;
        }

        storyboard.Begin();
    }

    /// <summary>複数の時点と値を指定してアニメーションを追加します。</summary>
    /// <param name="storyboard">追加先のストーリーボードです。</param>
    /// <param name="target">動かす要素または変形です。</param>
    /// <param name="property">動かすプロパティ名です。</param>
    /// <param name="frames">開始からのミリ秒と、その時点の値です。</param>
    private static void AddKeyFrames(
        Storyboard storyboard,
        DependencyObject target,
        string property,
        params (int Milliseconds, double Value)[] frames)
    {
        var animation = new DoubleAnimationUsingKeyFrames();
        foreach (var (milliseconds, value) in frames)
        {
            animation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(milliseconds)),
                Value = value,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
        }

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, property);
        storyboard.Children.Add(animation);
    }

    /// <summary>演出に合った形と色の紙ふぶきをボールから飛ばします。</summary>
    /// <param name="effect">今回のいたずらの効果です。</param>
    private void BurstConfetti(DrawEffect? effect)
    {
        _confettiStoryboard?.Stop();
        ConfettiLayer.Children.Clear();
        var storyboard = new Storyboard();
        _confettiStoryboard = storyboard;
        var (shapes, colors) = ConfettiTheme(effect);
        var count = effect == DrawEffect.Rainbow ? 32 : ConfettiCount;
        var center = ConfettiLayer.Width / 2;
        for (var index = 0; index < count; index++)
        {
            var angle = Math.PI * 2 * index / count + Random.Shared.NextDouble() * 0.4;
            var distance = 120 + Random.Shared.NextDouble() * 70;
            var transform = new CompositeTransform();
            var piece = new TextBlock
            {
                Text = shapes[index % shapes.Length],
                FontSize = 16 + Random.Shared.Next(14),
                Foreground = new SolidColorBrush(colors[index % colors.Length]),
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

        storyboard.Completed += (_, _) =>
        {
            if (ReferenceEquals(_confettiStoryboard, storyboard))
            {
                ConfettiLayer.Children.Clear();
                _confettiStoryboard = null;
            }
        };
        storyboard.Begin();
    }

    /// <summary>いたずらの種類に合う紙ふぶきの形と色を返します。</summary>
    /// <param name="effect">今回のいたずらの効果です。</param>
    /// <returns>紙ふぶきの形と色です。</returns>
    private static (string[] Shapes, Color[] Colors) ConfettiTheme(DrawEffect? effect) => effect switch
    {
        DrawEffect.SneakStart => (["♥", "♡", "✿"], [ConfettiColors[0], ConfettiColors[4]]),
        DrawEffect.Magic => (["✦", "✧", "★"], [ConfettiColors[3], ConfettiColors[1], ConfettiColors[4]]),
        DrawEffect.Magnet => (["✦", "●", "↗"], [ConfettiColors[5], ConfettiColors[3]]),
        DrawEffect.Gift => (["♥", "✿", "♡"], [ConfettiColors[0], ConfettiColors[4], ConfettiColors[1]]),
        DrawEffect.Stamp => (["✿", "◎", "♥"], [ConfettiColors[0], ConfettiColors[4]]),
        DrawEffect.Ufo => (["★", "✧", "●"], [ConfettiColors[2], ConfettiColors[3], ConfettiColors[5]]),
        DrawEffect.Cloud => (["☁", "♡", "✧"], [ConfettiColors[5], ConfettiColors[4]]),
        DrawEffect.Reverse => (["↺", "✦", "★"], [ConfettiColors[1], ConfettiColors[0]]),
        DrawEffect.Carrot => (["✿", "◆", "♥"], [ConfettiColors[1], ConfettiColors[2]]),
        _ => (["♥", "✦", "✿", "★", "♡", "●"], ConfettiColors)
    };

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
