using Microsoft.Web.WebView2.Wpf;
using NAudio.Wave;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

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

        private const int WinWidth = 500;
        private const int WinHeight = 600;

        WaveOut WaveOut;


        //Full:0,Withcontent:1
        private int displayMode = 0;

        private bool isCtrlPressed => GetKeyState(0xA2) < 0 || GetKeyState(0xA3) < 0;

        private WithContent WithContent { get; set; }
        private FullScreen FullScreen { get; set; }
        private Setting Setting { get; set; }
        private DispatcherTimer ClockTimer;
        private int ClockMode = 0;

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
            FullScreen.Time.Text = now.ToString("M/d     H:mm");
            ClockMode = 1 - ClockMode;
            FullScreen.Day.Text = ClockMode == 0 ? now.ToString("(ddd)") : UAUtil.GetUsDayOfWeek(now.DayOfWeek);

            WithContent.Time.Text = now.ToString("M/d     H:mm");
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
            if (e.Key == Key.Escape) this.Close();
        }

        private void SetPanel(int Id)
        {
            switch (Id)
            {
                case 0:
                    WithContent.INSTANCE.FloorName.Focus();
                    WithContent.INSTANCE.webView.Dispose();
                    WithContent.INSTANCE.webView = null;
                    var c = new Setting();
                    NavigationService.Navigate(c);
                    ResizeMode = ResizeMode.CanResize;
                    break;
                case 2:
                    WindowState = WindowState.Normal;
                    Width = 500;
                    Height = 600;
                    ResizeMode = ResizeMode.CanMinimize;
                    var wc = new WithContent();
                    NavigationService.Navigate(wc);
                    break;
            }
        }

        private string AudioResourceDirectoryPath => $"{Directory.GetCurrentDirectory()}\\resources\\";

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

        public async Task DoSimulation(SimulationContext context, int waittime)
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
            #endregion
            await Task.Delay(waittime * 1000);

            do
            {
                await ToNextFloor(context.startPos == 0 ? ElevatorDirection.UP : ElevatorDirection.DOWN, true, start);
                await Simu1(context, context.startPos == 0 ? ElevatorDirection.UP : ElevatorDirection.DOWN);
                await ToNextFloor(context.startPos == 0 ? ElevatorDirection.DOWN : ElevatorDirection.UP, context.RoundTrip, end);
                if (context.RoundTrip)
                {
                    await Simu1(context, context.startPos == 0 ? ElevatorDirection.DOWN : ElevatorDirection.UP);
                    if (!context.Loop)
                        await ToNextFloor(context.startPos == 0 ? ElevatorDirection.UP : ElevatorDirection.DOWN, false, start);
                }
            } while (context.Loop);

            BackToSetting();
        }

        private async Task ToNextFloor(ElevatorDirection nextDirection, bool toNextFloor, string arrivedFloor)
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
            await Task.Delay(5000);
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
            WithContent.IntroOrWarn.Source = new BitmapImage(UAUtil.RandomWarning);
            WithContent.webView.Visibility = Visibility.Hidden;
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
            WithContent.webView.Dispose();
            WithContent.webView = null;
            WithContent.Reset();
            NavigationService.Navigate(Setting);
            ClockTimer.Stop();
        }
    }
}
