using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace UrbanAce_7
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:UrbanAce_7"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:UrbanAce_7;assembly=UrbanAce_7"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:NumericBox/>
    ///
    /// </summary>
    public class NumericBox : Control
    {
        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register(
                "Value", typeof(int), typeof(NumericBox));
        private TextBox MainBox;

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        static NumericBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericBox), new FrameworkPropertyMetadata(typeof(NumericBox)));
        }

        public void Check(object sender, TextCompositionEventArgs e) =>
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);

        public void PasteCheck(object sender, KeyEventArgs e) => 
            e.Handled = Win32API.isCtrlPressed && e.Key == Key.V && !new Regex("[0-9]").IsMatch(Clipboard.GetText());

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (MainBox != null)
            {
                MainBox.PreviewTextInput -= Check;
                MainBox.PreviewKeyDown -= PasteCheck;
            }

            MainBox = GetTemplateChild("MainText") as TextBox;
            if (MainBox != null)
            {
                MainBox.PreviewTextInput += Check;
                MainBox.PreviewKeyDown += PasteCheck;
                MainBox.ContextMenu = null;
            }
        }

        public void Clear()
        {
            if (MainBox == null)
                MainBox = GetTemplateChild("MainText") as TextBox;
            Value = 0;
            MainBox.Clear();
        }
    }
}
