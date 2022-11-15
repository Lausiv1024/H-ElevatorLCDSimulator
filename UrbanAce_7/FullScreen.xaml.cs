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

namespace UrbanAce_7
{
    /// <summary>
    /// FullScreen.xaml の相互作用ロジック
    /// </summary>
    public partial class FullScreen : Page
    {
        readonly SolidColorBrush f = new SolidColorBrush(MainDesign.TextC);
        readonly SolidColorBrush b = new SolidColorBrush(MainDesign.Background);
        readonly string FLOOR_FONT = "Segoe UI";
        readonly string INFO_US_FONT = "Segoe_UI";
        readonly string INFO_JP_FONT = "BIZ UDゴシック";
        public static FullScreen Instance { get; private set; }
        public static bool IsInstanceCreated => Instance != null;
        public FullScreen()
        {
            PreInit();
            InitializeComponent();
            Init();
            Instance = this;
        }
        private void PreInit()
        {

        }

        private void Init()
        {

        }
        private void PostInit()
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PostInit();
        }
    }
}
