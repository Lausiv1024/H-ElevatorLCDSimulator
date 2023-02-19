using Microsoft.Web.WebView2.Wpf;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UrbanAce_7.ContentSettings;

namespace UrbanAce_7
{
    /// <summary>
    /// WithContent.xaml の相互作用ロジック
    /// </summary>
    public partial class WithContent : Page
    {
        readonly SolidColorBrush f = new SolidColorBrush(MainDesign.TextC);
        readonly SolidColorBrush b = new SolidColorBrush(MainDesign.Background);
        readonly string FLOOR_FONT = "Segoe UI";
        readonly string INFO_US_FONT = "Segoe_UI";
        readonly string INFO_JP_FONT = "BIZ UDゴシック";
        public static WithContent INSTANCE { get; private set; }
        public static bool isInstanceCreated => INSTANCE != null;
        public WebView2 webView;
        public ElevatorDirection direction { get; private set; }
        private double ArrowImgSize => ArrowRenderer.ActualWidth;

        private int arMotion = 0;
        private bool isAnimating;

        private int InfoLang = 0;//0:JP1:US
        TranslatableInfoText curInfoText;
        DispatcherTimer infoUpdateTimer;
        DispatcherTimer ArrowTimer;
        private const int ArrowSlow = 1800;
        private const int ArrowFast = 900;

        public List<UABaseContent> Contents = new List<UABaseContent>();
        private DispatcherTimer ContentUpdateTimer = new DispatcherTimer();
        int prevContentIndex = -1;
        const int DefaultContentChangeTime = 8;

        public int ArrowMotion
        {
            get { return arMotion; }
            set
            {
                arMotion = value;
                ArrowTimer.IsEnabled = value != 0;
                if (value == 2)
                {
                    ArrowTimer.Interval = TimeSpan.FromMilliseconds(ArrowFast);
                    DoArrowAnim();
                }else if (value == 1)
                {
                    ArrowTimer.Interval = TimeSpan.FromMilliseconds(ArrowSlow);
                    DoArrowAnim();
                }
            }
        }

        public WithContent()
        {
            PreInit();
            InitializeComponent();
            Init();
        }
        private void PreInit()//環境変数の指定は要素の初期化が完了する前にやる
        {
            INSTANCE = this;
            webView = new WebView2();
            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
                "--autoplay-policy=no-user-gesture-required");
            Loaded += async (s, e) => await PostInit();
            ArrowTimer = new DispatcherTimer();
            ArrowTimer.Tick += (s, e) => DoArrowAnim();
        }
        private void Init()
        {
            DataContext = new DesignData { Foreground = f, Background = b };//背景、メインの色を設定
            FloorName.FontFamily = new FontFamily(FLOOR_FONT);
            FloorName.FontWeight = FontWeights.SemiBold;
            FloorName.FontStyle = FontStyles.Italic;
            InfoGrid.Children.Add(webView);
            direction = ElevatorDirection.UP;
        }
        private async Task PostInit()//PostInit is called on Loaded event
        {
            Clear__DEBUG_C();

            drawArrow(direction == ElevatorDirection.DOWN);
            elementFadeIn(ArrowRenderer.Children[0]);
            elementFadeIn(FloorName);
            setInfoText(TranslatableInfoText.NextFloor);
            if (webView is null) 
            {
                webView = new WebView2();
                InfoGrid.Children.Add(webView);
            }
            webView.CoreWebView2InitializationCompleted += (s, a) =>
            {
                if (!a.IsSuccess) return;
                webView.IsEnabled = false;
            };
            IntroOrWarn.Source = new BitmapImage(UAUtil.RandomIntro);
            IntroOrWarn.Visibility = Visibility.Visible;
            await Task.Delay(10);
            infoUpdateTimer = new DispatcherTimer();
            infoUpdateTimer.Interval = new TimeSpan(0, 0, 4);
            infoUpdateTimer.Tick += (s, e) =>
            {
                InfoLang = 1 - InfoLang;
                setInfoText(curInfoText);
            };
            infoUpdateTimer.Start();
            UpdateClock();
            ContentUpdateTimer.Stop();
            ContentUpdateTimer.Interval = TimeSpan.FromSeconds(DefaultContentChangeTime);
            ContentUpdateTimer.Tick += async (s, e) =>
            {
                await UpdateContent();
            };
            ContentUpdateTimer.Start();
        }

        private async Task UpdateContent()
        {
            int i;
            i = UAUtil.Rand.Next(0, Contents.Count + 1);


            if (i == Contents.Count || Contents.Count == 0)
            {
                News.Visibility = Visibility.Hidden;
                IntroOrWarn.Source = new BitmapImage(UAUtil.RandomIntro);
                IntroOrWarn.Visibility = Visibility.Visible;
                return;
            }
            
            HideAllCustomContents();
            var info = Contents[i];
            //if (!(info is YoutubeEmbed)) ContentUpdateTimer.Interval = TimeSpan.FromSeconds(DefaultContentChangeTime);
            
            if (info is CustomNews)
            {
                var cNews = (CustomNews)info;
                NewsText.Text = cNews.newsText.Text;
                ProvidedBy.Text = $"{cNews.providedBy.Text} 提供";
                News.Visibility = Visibility.Visible;
            } else if (info is Media)
            {
                var media = (Media)info;
                if (webView == null) 
                { 
                    webView = new WebView2();
                    InfoGrid.Children.Add(webView);
                } else
                {
                    webView.Visibility = Visibility.Visible;
                }
                await setWebView(media.Path.Text);
            } else if (info is YoutubeEmbed)
            {
                var yEmbed = (YoutubeEmbed)info;
                //ContentUpdateTimer.Interval = TimeSpan.FromSeconds(yEmbed.viewTime.Value);
                if (webView == null) 
                {
                    webView = new WebView2();
                    InfoGrid.Children.Add(webView);
                } else
                {
                    webView.Visibility = Visibility.Visible;
                }

                await setYoutubeEnbedContent(yEmbed.MovID.Text);
            }else if (info is RandomTL)
            {
                if (UAUtil.TLTweets.Length == 0) return;
                var tweet = UAUtil.TLTweets[UAUtil.Rand.Next(0, UAUtil.TLTweets.Length)];
                TweetTitle.Text = $"{tweet.CreatedBy.Name}さんのツイート";
                TweetText.Text = tweet.FullText;
                Tweet.Visibility = Visibility.Visible;
            }else if (info is UserTweet)
            {
                if (UAUtil.UserTweets.Count == 0) return;
                var tweet = UAUtil.UserTweets[UAUtil.Rand.Next(0, UAUtil.UserTweets.Count)];
                TweetTitle.Text = $"{tweet.CreatedBy.Name}さんのツイート";
                TweetText.Text = tweet.FullText;
                Tweet.Visibility = Visibility.Visible;
            }
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            Time.Text = now.ToString("M/d      H:mm");
        }

        public void onArrive()
        {
            ContentUpdateTimer.Stop();
            IntroOrWarn.Source = new BitmapImage(UAUtil.RandomWarning);
            HideAllCustomContents();
        }

        private void HideAllCustomContents()
        {
            if (!(webView is null)) webView.Visibility = Visibility.Hidden;
            News.Visibility = Visibility.Hidden;
            Tweet.Visibility= Visibility.Hidden;
        }

        public void drawArrow(bool isDown)
        {
            Image img = createArrowImg(ArrowImgSize);
            if (isDown)
            {
                RotateTransform t = new RotateTransform(180);
                t.CenterX = ArrowImgSize / 2;
                t.CenterY = ArrowImgSize / 2;
                img.RenderTransform = t;
            }
            Add__DEBUG_C(img);
        }

        private Image createArrowImg(double size, double rotation)
        {
            var img = new Image();
            img.Source = new BitmapImage(Arrows.LIGHT_THEME);
            img.Width = size;
            img.Height = size;
            RotateTransform t = new RotateTransform(rotation);
            t.CenterX = ArrowImgSize / 2;
            t.CenterY = ArrowImgSize / 2;
            img.RenderTransform = t;
            return img;
        }

        private int infoMargin => InfoLang == 0 ? 12 : 4;
        private int infoFontSize => InfoLang == 0 ? 42 : 50;
        public void setInfoText(TranslatableInfoText txt)
        {
            curInfoText = txt;
            Info1.FontFamily = InfoLang == 0 ? new FontFamily(INFO_JP_FONT) : new FontFamily(INFO_US_FONT);
            Info1.FontWeight = InfoLang == 0 ? FontWeights.Bold : FontWeights.SemiBold;
            Info1.Margin = new Thickness(infoMargin);
            Info1.FontSize = infoFontSize;

            Info1.Text = InfoLang == 0 ? txt.JP : txt.US;
            if (txt.infoType == TranslatableInfoText.InfoType.FLOOR)
            {
                NextFloor.Visibility = Visibility.Visible;
            } else
            {
                NextFloor.Visibility = Visibility.Hidden;
            }
            if (int.TryParse(NextFloor.Text, out int floorNum) && InfoLang == 1 && curInfoText.infoType == TranslatableInfoText.InfoType.FLOOR)
            {
                int spaceCount = (int)(8 + NextFloor.Text.Length * 1.6);
                string ordText = "";
                for (int i = 0; i < spaceCount; i++) ordText += " ";
                ordText += UAUtil.Ordinal(floorNum);
                Ord.Text = ordText;
            } else Ord.Text = string.Empty;
        }

        private Image createArrowImg(double size) => createArrowImg(size, 0);
        private void elementFadeIn(UIElement element)
        {
            element.Opacity = 0;
            var storyBoard = new Storyboard();
            var da = new DoubleAnimation();
            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
            da.From = 0;
            da.To = 1.0;
            da.Duration = TimeSpan.FromMilliseconds(200);
            storyBoard.Children.Add(da);

            storyBoard.Begin();
        }

        private void elementFadeOut(UIElement element, int millSec) => elementFadeOut(element, millSec, null);

        /// <summary>
        /// UI要素をフェードアウトさせる。
        /// </summary>
        /// <param name="element"></param>
        /// <param name="onFadeouted"></param>
        private void elementFadeOut(UIElement element,int millSec, Action onFadeouted)
        {
            element.Opacity = 1;
            var storyBoard = new Storyboard();
            var da = new DoubleAnimation();
            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
            da.From = 1.0;
            da.To = 0.0;
            da.Duration = TimeSpan.FromMilliseconds(millSec);
            storyBoard.Completed += (s, e) =>
            {
                onFadeouted?.Invoke();
            };
            storyBoard.Children.Add(da);
            storyBoard.Begin();
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }


        private void DisposeWebview()
        {
            if (webView != null)
            {
                webView.Dispose();
                webView = null;
            }
        }

        public void updateFloor(string fl)
        {
            setFloorText("");
            delayDo(50, () =>
            {
                setFloorText(fl);
                elementFadeIn(FloorName);
            });
        }

        private void delayDo(double millSec, Action a)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(millSec);
            timer.Tick += (s, e) =>
            {
                a?.Invoke();
                timer.Stop();
            };
            timer.Start();
        }

        //いいねこの書き方
        private const int d = 160;
        private const int mvTime = 800;
        private int NextArrowStartPos => direction == ElevatorDirection.UP ? d : -d;
        private int PrevArrowEndPos => direction == ElevatorDirection.UP ? -d : d;

        public void DoArrowAnim()
        {
            if (direction == ElevatorDirection.NONE || ArrowRenderer.Children.Count == 0 || isAnimating) return;
            isAnimating = true;
            var prevArrow = ArrowRenderer.Children[0] as Image;
            var nextArrow = createArrowImg(ArrowImgSize, direction == ElevatorDirection.DOWN ? 180 : 0);

            Canvas.SetTop(nextArrow, NextArrowStartPos);
            Add__DEBUG_C(nextArrow);
            MoveElementTop(prevArrow, 0, PrevArrowEndPos, mvTime, () =>
            {
                Remove__DEBUG_C(prevArrow);
                isAnimating = false;
            });
            MoveElementTop(nextArrow, NextArrowStartPos, 0, mvTime);
        }



        private void MoveElementTop(UIElement element, double from, double to, double millSecDuration) =>
            MoveElementTop(element, from, to, millSecDuration, null);
        private void MoveElementTop(UIElement element, double from, double to, double millSecDuration, Action onCompleted)
        {
            var sBoard = new Storyboard();
            var dA = new DoubleAnimation();

            var easeFunc = new CubicEase();
            easeFunc.EasingMode = EasingMode.EaseInOut;

            dA.EasingFunction = easeFunc;

            Storyboard.SetTarget(dA, element);
            Storyboard.SetTargetProperty(dA, new PropertyPath(Canvas.TopProperty));
            dA.From = from; dA.To = to;
            dA.Duration = TimeSpan.FromMilliseconds(millSecDuration);

            sBoard.Children.Add(dA);
            sBoard.Completed += (sender, e) =>
            {
                onCompleted?.Invoke();
            };
            sBoard.Begin();
        }

        public void UpdateArrow(ElevatorDirection d)
        {
            if (direction == d) return;
            direction = d;
            if (d == ElevatorDirection.NONE)
            {
                if (ArrowRenderer.Children.Count == 0) return;
                elementFadeOut(ArrowRenderer.Children[0],200, () => Clear__DEBUG_C());
            } else
            {
                Clear__DEBUG_C();
                delayDo(50, () =>
                {
                    drawArrow(d == ElevatorDirection.DOWN);
                    var img = ArrowRenderer.Children[0] as Image;
                    elementFadeIn(img);
                });
                
            }
        }

        private void setFloorText(string str)
        {
            bool disableMargin = str.Length > 1;
            Thickness t = disableMargin ? new Thickness(-24, -20, 10, 0) : new Thickness(16, -20, 60, 0);
            FloorName.Text = str;
            FloorName.Margin = t;
        }

        public async Task setWebView(string url)
        {
            if (this.webView.CoreWebView2 is null)
                await this.webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.IsMuted = true;
            this.webView.CoreWebView2.Navigate(url);
        }
        public async Task setYoutubeEnbedContent(string MovID)
        {
            if (this.webView.CoreWebView2 is null)
                await webView.EnsureCoreWebView2Async();
            string completedHTML = Properties.Resources.EmbedMute.Replace("{movieID}", MovID);
            webView.CoreWebView2.NavigateToString(completedHTML);
        }

        public async Task FadeOut()
        {
            if (ArrowRenderer.Children.Count != 0)
                elementFadeOut(ArrowRenderer.Children[0],100);
            elementFadeOut(FloorName,100);
            await Task.Delay(400);
            Reset();
        }

        public void Reset()
        {
            Clear__DEBUG_C();
            ArrowRenderer.Children.Capacity = 1;
            infoUpdateTimer?.Stop();
            ArrowMotion = 0;

        }

        private void Add__DEBUG_C(UIElement e)
        {
            ArrowRenderer?.Children.Add(e);
        }

        private void Remove__DEBUG_C(UIElement e)
        {
            ArrowRenderer?.Children.Remove(e);
        }

        private void Clear__DEBUG_C()
        {
            ArrowRenderer?.Children.Clear();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeWebview();
        }
    }
}
