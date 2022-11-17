using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

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


        //Setting : 0  Full : 1  WithContents:2
        private int displayMode = 2;

        private bool isCtrlPressed => GetKeyState(0xA2) < 0 || GetKeyState(0xA3) < 0;

        private WithContent WithContent { get; set; }
        private FullScreen FullScreen { get; set; }

        private bool IsResizable { get
            {
                return ResizeMode == ResizeMode.CanResize;
            }
            set {
                ResizeMode = value ? ResizeMode.CanResize : ResizeMode.CanMinimize;
                if (!value) 
                {
                    WindowState = WindowState.Normal;
                    Height = WinHeight;
                    Width = WinWidth;
                }
            }
        }

        private bool animationRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            WithContent = new WithContent();
            FullScreen = new FullScreen();
            NavigationService.Navigate(FullScreen);
        }

        private async void NavigationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await deleteWebViewData();
            try
            {
                if (WithContent.INSTANCE == null || WithContent.INSTANCE.Content != null) return;
                WithContent.INSTANCE.webView.Dispose();
            } catch{}
        }

        private async Task deleteWebViewData()
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
            if (e.Key == Key.S && isCtrlPressed)
            {
                if (animationRunning) return;
                animationRunning = true;
                IsResizable = false;
                if (displayMode == 0) displayMode = 1;
                else displayMode = displayMode == 1 ? 2 : 1;
                if (displayMode == 2)
                {
                    FullScreen.FadeOut();
                    await Task.Delay(600);
                    NavigationService.Navigate(WithContent);
                } else if (displayMode == 1)
                {
                    await WithContent.FadeOut();
                    NavigationService.Navigate(FullScreen);
                }
                animationRunning = false;
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
    }
}
