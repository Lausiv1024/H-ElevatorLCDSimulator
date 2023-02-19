using Microsoft.Web.WebView2.Wpf;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using UrbanAce_7.ContentSettings;

namespace UrbanAce_7
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        public static readonly string MainTitle = "Urban Ace";
        public static MainWindow Instance { get; private set; }

        private const int WinWidth = 520;
        private const int WinHeight = 620;

        WaveOut WaveOut;

        private bool loopCanceled;

        //Full:0,Withcontent:1
        private int displayMode = 0;

        private bool isCtrlPressed => GetKeyState(0xA2) < 0 || GetKeyState(0xA3) < 0;

        private WithContent WithContent { get; set; }
        private FullScreen FullScreen { get; set; }
        private Setting Setting { get; set; }
        private DispatcherTimer ClockTimer;
        private int ClockMode = 0;

        private SimulationContext Current;

        private bool IsResizable
        {
            set
            {
                ResizeMode = value ? ResizeMode.CanResize : ResizeMode.CanMinimize;
                if (!value)
                {
                    WindowState = WindowState.Normal;
                    Height = WinHeight;
                    Width = WinWidth;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            WithContent = new WithContent();
            FullScreen = new FullScreen();
            Setting = new Setting();
            NavigationService.Navigate(Setting);
            IsResizable = true;
            WaveOut = new WaveOut();
            ClockTimer = new DispatcherTimer();
            ClockTimer.Interval = new TimeSpan(0, 0, 4);
            ClockTimer.Tick += (s, e) => updateClock();

            if (!File.Exists(UAUtil.ResourceDirectoryPath)) 
                Directory.CreateDirectory(UAUtil.ResourceDirectoryPath);
        }

        private async void NavigationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await DeleteWebViewData();
            try
            {
                if (WithContent == null || WithContent.Content != null) return;
                WithContent.webView.Dispose();
            } catch { }
        }

        private async Task DeleteWebViewData()
        {
            if (!WithContent.isInstanceCreated) return;
            WebView2 webview = WithContent.INSTANCE.webView;
            try
            {
                if (webview == null || webview.CoreWebView2 is null) return;
                
                var p = webview.CoreWebView2.Profile;
                await p.ClearBrowsingDataAsync();
            } catch (ObjectDisposedException de)
            {
                Console.WriteLine($"Webview is Disposed : Msg : {de}");
            }
        }
        private void updateClock()
        {
            var now = DateTime.Now;
            FullScreen.Time.Text = now.ToString("M/d      H:mm");
            ClockMode = 1 - ClockMode;
            FullScreen.Day.Text = ClockMode == 0 ? now.ToString("(ddd)") : UAUtil.GetUsDayOfWeek(now.DayOfWeek);

            WithContent.Time.Text = now.ToString("M/d      H:mm");
            WithContent.Day.Text = ClockMode == 0 ? now.ToString("(ddd)") : UAUtil.GetUsDayOfWeek(now.DayOfWeek);

        }
        private async void NavigationWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && isCtrlPressed)
            {
                string a = Clipboard.GetText().Replace(Environment.NewLine, "");
                WebView2 web = WithContent.INSTANCE.webView;
                if (web == null) return;
                if (a.Contains("https://"))
                {
                    await WithContent.INSTANCE.setWebView(a);
                } else if (a.Length == 11)
                {
                    await WithContent.INSTANCE.setYoutubeEnbedContent(a);
                }
            }
            if (e.Key == Key.F && isCtrlPressed)
            {
                if (displayMode == 0) return;
                displayMode = 0;
                IsResizable = true;
                Title = displayMode == 0 ? $"{MainTitle} -- 設定" : MainTitle;
                var setting = new Setting();
                NavigationService.Navigate(setting);
            }
            if (e.Key == Key.K)
            {
                ElevatorDirection d = WithContent.INSTANCE.direction;
                WithContent.INSTANCE.UpdateArrow(d == ElevatorDirection.UP ? ElevatorDirection.DOWN :
                    ElevatorDirection.UP);
            }
            if (e.Key == Key.L)
            {
                WithContent.INSTANCE.UpdateArrow(ElevatorDirection.NONE);
            }
            if (e.Key == Key.M)
            {
                WithContent.INSTANCE.DoArrowAnim();
            }
            if (e.Key == Key.Escape)
            {
                if (Current == null) return;
                if (!Current.Loop) return;
                loopCanceled = true;
                WithContent.LoopMode.Text = "Loop Canceled";
                await Task.Delay(500);
                WithContent.LoopMode.Text = "";
            }
        }

        private string AudioResourceDirectoryPath => Path.Combine(UAUtil.BaseDir, $"resources{Path.DirectorySeparatorChar}");

        private bool PlayFloorArriveSound(string floorName)
        {
            string filePath = AudioResourceDirectoryPath + floorName + ".mp3";
            return PlaySound(filePath);
        }

        private void PlayFloorArriveSoundAndPlayOpen(string floorName)
        {
            if (PlayFloorArriveSound(floorName))
                WaveOut.PlaybackStopped += wait;
        }

        private async void wait(object s, StoppedEventArgs e)
        {
            await Task.Delay(800);
            PlayDoorAnnounce(0);
            WaveOut.PlaybackStopped -= wait;
        }

        private void PlayUpDownSound(int mode)
        {
            string type = mode == 0 ? "Up" : "Down";
            string filePath = AudioResourceDirectoryPath + type + ".mp3";
            PlaySound(filePath);
        }

        private void PlayDoorAnnounce(int mode)
        {
            string type = mode == 0 ? "Open" : "Close";
            string filePath = AudioResourceDirectoryPath +
                type + ".mp3";
            PlaySound(filePath);
        }
        private bool PlaySound(string path)
        {
            if (!File.Exists(path)) return false;
            var aReader = new AudioFileReader(path);
            WaveOut.Init(aReader);
            WaveOut.Play();
            return true;
        }

        private async Task SwitchDisplay(int mode)
        {
            if (mode == 1)
            {
                FullScreen.FadeOut();
                await Task.Delay(600);
                NavigationService.Navigate(WithContent);
            } else
            {
                await WithContent.FadeOut();
                await Task.Delay(200);
                NavigationService.Navigate(FullScreen);
                WithContent.IntroOrWarn.Visibility = Visibility.Collapsed;
                if (WithContent.webView != null)
                    WithContent.webView.Visibility = Visibility.Visible;
            }
        }

        public async Task DoSimulation(SimulationContext context, int waittime, List<UABaseContent> contents)
        {
            #region 初期化処理
            updateClock();
            ClockTimer.Start();
            bool startFromBottom = context.startPos == 0;
            string start = startFromBottom ? context.AvailableFloors[0] : context.AvailableFloors[context.AvailableFloors.Length - 1];
            string end = !startFromBottom ? context.AvailableFloors[0] : context.AvailableFloors[context.AvailableFloors.Length - 1];
            WithContent.updateFloor(start);

            NavigationService.Navigate(FullScreen);
            IsResizable = false;
            await Task.Delay(waittime * 1000);
            WithContent.Contents = contents;
            bool hasTwitterFunc = UABaseContent.hasContentRequireTwitterAPI(contents);
            int count = 0;
            Current = context;
            loopCanceled = false;
            #endregion
            do
            {
                await ToNextFloor(context.startPos == 0 ? ElevatorDirection.UP : ElevatorDirection.DOWN, true, start,
                    hasTwitterFunc && count > 0 ? contents.Where(c => c.requireAuth).ToList() : null);
                await Simu1(context, context.startPos == 0 ? ElevatorDirection.UP : ElevatorDirection.DOWN);
                await ToNextFloor(context.startPos == 0 ? ElevatorDirection.DOWN : ElevatorDirection.UP, context.RoundTrip, end);
                if (context.RoundTrip)
                {
                    await Simu1(context, context.startPos == 0 ? ElevatorDirection.DOWN : ElevatorDirection.UP);
                    if (!context.Loop)
                        await ToNextFloor(context.startPos == 0 ? ElevatorDirection.UP : ElevatorDirection.DOWN, false, start);
                }
                count++;
            } while (context.Loop && !loopCanceled);

            BackToSetting();
        }

        private async Task ToNextFloor(ElevatorDirection nextDirection, bool toNextFloor, string arrivedFloor) =>
            await ToNextFloor(nextDirection, toNextFloor, arrivedFloor, null);

        private async Task ToNextFloor(ElevatorDirection nextDirection, bool toNextFloor,
            string arrivedFloor, List<UABaseContent> twitterContents)
        {
            FullScreen.Direction = nextDirection;
            FullScreen.FloorText.Text = arrivedFloor;
            FullScreen.updateArrow(nextDirection);
            WithContent.UpdateArrow(nextDirection);

            await SwitchDisplay(0);
            WithContent.setInfoText(TranslatableInfoText.Empty);
            if (toNextFloor)
            {
                await Task.Delay(2500);
                PlayUpDownSound(nextDirection == ElevatorDirection.UP ? 0 : 1);
            }
            if (twitterContents != null && twitterContents.Count != 0)
            {
                if (twitterContents.Where(c => c is RandomTL).Count() > 0)
                {
                    await UAUtil.GetTimeLine(UAUtil.AuthData);
                }
                UAUtil.UserTweets.Clear();
                var users = twitterContents.Where(c => c is UserTweet).ToList();
                int count = users.Count;
                users.ForEach(async c =>
                {
                    var u = c as UserTweet;
                    await UAUtil.GetUserTweets(UAUtil.AuthData, u.UserName);
                    await Task.Delay(500);
                    count--;
                });
                await Task.Run(() =>
                {
                    while (count > 0)
                    {
                        Thread.Sleep(500);
                    }
                });
            } else
            {
                await Task.Delay(5000);
            }
            if (toNextFloor)
            {
                FullScreen.UpdateInfoText(TranslatableInfoText.DoorClose);
                PlayDoorAnnounce(1);
                await Task.Delay(5200);
                FullScreen.UpdateInfoText(TranslatableInfoText.Empty);
            }
        }

        private async Task Simu1(SimulationContext context, ElevatorDirection direction)
        {
            bool startFromBottom = direction == ElevatorDirection.UP;
            string end = !startFromBottom ? context.AvailableFloors[0] : context.AvailableFloors[context.AvailableFloors.Length - 1];
            await SwitchDisplay(1);
            WithContent.ArrowMotion = 2;
            WithContent.NextFloor.Text = end;

            if (startFromBottom)
            {
                for (int i = 1; i < context.AvailableFloors.Length; i++)
                {
                    await Task.Delay(3000);
                    WithContent.updateFloor(context.AvailableFloors[i]);
                }
            } else
            {
                for (int i = context.AvailableFloors.Length - 2; i >= 0; i--)
                {
                    await Task.Delay(3000);
                    WithContent.updateFloor(context.AvailableFloors[i]);
                }
            }
            await Task.Delay(1000);
            WithContent.ArrowMotion = 1;
            PlayFloorArriveSoundAndPlayOpen(end);
            await Task.Delay(1200);
            WithContent.onArrive();
            WithContent.IntroOrWarn.Visibility = Visibility.Visible;
            WithContent.setInfoText(TranslatableInfoText.DoorOpen);
            await Task.Delay(3000);
            WithContent.ArrowMotion = 0;
            await Task.Delay(1000);
            WithContent.UpdateArrow(startFromBottom ? ElevatorDirection.DOWN : ElevatorDirection.UP);
            await Task.Delay(200);
        }

        private void BackToSetting()
        {
            if (!(WithContent.webView is null))
                WithContent.webView.Dispose();
            Current = null;
            WithContent.webView = null;
            IsResizable = true;
            WithContent.Reset();
            NavigationService.Navigate(Setting);
            ClockTimer.Stop();
        }
    }
}
