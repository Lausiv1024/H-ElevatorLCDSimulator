﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UrbanAce_7
{
    /// <summary>
    /// FullScreen.xaml の相互作用ロジック
    /// </summary>
    public partial class FullScreen : Page
    {
        readonly SolidColorBrush f = new SolidColorBrush(MainDesign.TextC);
        readonly SolidColorBrush b = new SolidColorBrush(MainDesign.Background);
        readonly string INFO_US_FONT = "Segoe_UI";
        readonly string INFO_JP_FONT = "BIZ UDゴシック";
        public int InfoLang = 0;
        private TranslatableInfoText curInfoText = TranslatableInfoText.Empty;
        public ElevatorDirection Direction;

        public static FullScreen Instance { get; private set; }
        public static bool IsInstanceCreated => Instance != null;

        public int ClockMode = 0;
        
        public FullScreen()
        {
            PreInit();
            InitializeComponent();
            Init();
            Instance = this;
        }

        private void PreInit()
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 4);
            timer.Tick += (s, e) =>
            {
                InfoLang = 1 - InfoLang;
                UpdateInfoText(curInfoText);
            };
            timer.Start();
            Direction = ElevatorDirection.UP;
        }

        private void Init()
        {
            DataContext = new DesignData { Foreground = f, Background = b };
            FloorText.Opacity = 0;
        }
        private async void PostInit(object sender, RoutedEventArgs e)
        {
            SwitchElements(Direction);
            updateArrow(Direction);
            ArrowRenderer.Children[0].Opacity = 0;
            await Task.Delay(300);
            FadeElement(FloorText, 200, UAUtil.FadeType.IN);
            FadeElement(ArrowRenderer.Children[0], 200, UAUtil.FadeType.IN);
        }

        private double ArrowImgSize => ArrowRenderer.ActualWidth;

        private Image CreateArrowImg(double size, double rotation)
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

        private void SwitchElements(ElevatorDirection direction)
        {
            Grid.SetRow(FloorText, direction == ElevatorDirection.UP ? 2 : 1);
            Grid.SetRow(ArrowRenderer, direction == ElevatorDirection.UP ? 1 : 2);
        }

        private int infoFontSize => InfoLang == 0 ? 42 : 50;
        private int infoMargin => InfoLang == 0 ? 20 : 12;

        public void UpdateInfoText(TranslatableInfoText text)
        {
            curInfoText = text;
            InfoText.FontFamily = InfoLang == 0 ? new FontFamily(INFO_JP_FONT) : new FontFamily(INFO_US_FONT);
            InfoText.FontWeight = InfoLang == 0 ? FontWeights.Bold : FontWeights.SemiBold;
            InfoText.Margin = new Thickness(infoMargin);
            InfoText.FontSize = infoFontSize;
            InfoText.Text = InfoLang == 0 ? text.JP : text.US;
        }

        private void FadeElement(UIElement element, double time, UAUtil.FadeType FadeType)
        {
            if (FadeType == UAUtil.FadeType.IN) element.Opacity = 0;
            var storyboard = new Storyboard();
            var da = new DoubleAnimation();
            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
            da.From = FadeType == UAUtil.FadeType.IN ? 0 : 1;
            da.To = FadeType == UAUtil.FadeType.IN ? 1 : 0;
            da.Duration = TimeSpan.FromMilliseconds(time);
            storyboard.Children.Add(da);
            storyboard.Begin();
        }

        public void updateArrow(ElevatorDirection direction)
        {
            Direction = direction;
            ArrowRenderer.Children.Clear();
            var a = CreateArrowImg(ArrowImgSize, Direction == ElevatorDirection.DOWN ? 180 : 0);
            ArrowRenderer.Children.Add(a);
            SwitchElements(direction);
        }

        public void FadeOut()
        {
            FadeElement(FloorText, 100, UAUtil.FadeType.OUT);
            if (ArrowRenderer.Children.Count != 0)
                FadeElement(ArrowRenderer.Children[0], 100, UAUtil.FadeType.OUT);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
