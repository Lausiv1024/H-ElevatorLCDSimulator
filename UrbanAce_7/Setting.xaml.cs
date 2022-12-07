using System;
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
using System.Text.RegularExpressions;
using System.IO;
using UrbanAce_7.ContentSettings;
using System.Collections.Generic;
using Tweetinvi;
using Newtonsoft.Json;
using System.Collections;
using Microsoft.Win32;
using UrbanAce_7.Profiling;

namespace UrbanAce_7
{
    /// <summary>
    /// Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Setting : Page
    {
        private int startPos = 0;//0:UP 1:DW

        public Setting()
        {
            InitializeComponent();
            FloorS.PreviewTextInput += (s, e) =>
            {
                e.Handled = (FloorS.Text.Length + e.Text.Length) > 3;
            };
            addEnterAction(FloorS, () => AddSingleFloor(FloorS.Text, false));
            foreach (int s in UAUtil.Speeds) Speed.Items.Add(s);
            InfoList.SelectionChanged += InfoList_Selected;
            LoadAuthData();
        }

        private void FloorS_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                var cText = Clipboard.GetText();
                e.Handled = (FloorS.Text.Length + cText.Length) > 3 || !new Regex("[0-9A-Za-z]").IsMatch(cText);
            }
        }

        private void addEnterAction(UIElement element, Action a)
        {
            element.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) a?.Invoke();
            };
        }

        private void AddFloor_Click(object sender, RoutedEventArgs e) => AddSingleFloor(FloorS.Text, false);
        private void AddSingleFloor(string text, bool ignoreEmpty)
        {
            if ((FloorS.Text.Length == 0 && !ignoreEmpty) || CheckAlreadyExists(text)) return;
            if (FloorList.SelectedIndex == -1)
                FloorList.Items.Add(createListBoxItem(text));
            else FloorList.Items.Insert(FloorList.SelectedIndex, createListBoxItem(text));
            FloorS.Clear();
            SetTopAndBottomFloor();
        }

        private ListBoxItem createListBoxItem(string text)
        {
            var i = new ListBoxItem();
            i.Content = text;
            i.ContextMenu = new ContextMenu();
            var del = new MenuItem();
            del.Header = "消去";
            del.Click += (s, e) =>
            {
                if (FloorList.SelectedIndex < 0) return;
                FloorList.Items.RemoveAt(FloorList.SelectedIndex);
                SetTopAndBottomFloor();
            };

            var addToMiddlefloorList = new MenuItem();
            addToMiddlefloorList.Header = "中間停止階として設定";
            addToMiddlefloorList.Click += (s, e) =>
            {
                CreateMiddleFloorItem(GetFloorName(FloorList.SelectedIndex));
            };

            i.ContextMenu.Items.Add(del);
            i.ContextMenu.Items.Add(addToMiddlefloorList);
            return i;
        }

        private ListBoxItem Copy(ListBoxItem item) => createListBoxItem((string)item.Content);

        private bool CheckAlreadyExists(string Name)
        {
            foreach (var item in FloorList.Items)
            {
                var listboxItem = item as ListBoxItem;
                if ((string)listboxItem.Content == Name) return true;
            }
            return false;
        }

        private string GetFloorName(int index)
        {
            var a = FloorList.Items[index] as ListBoxItem;
            return (string) a.Content;
        }

        private void BasementFloor_Click(object sender, RoutedEventArgs e) => AddSerialFloor(true);
        private void AddSerial_Click(object sender, RoutedEventArgs e) => AddSerialFloor(false);

        public void AddSerialFloor(bool isBasement)
        {
            int start = Math.Min(StartF.Value, EndF.Value);
            int end = Math.Max(StartF.Value, EndF.Value);
            if (start == 0 && end == 0) return;
            addSerialFloors(start, end + 1, isBasement ? "B" : "");
            StartF.Clear();
            EndF.Clear();
            SetTopAndBottomFloor();
        }

        private void addSerialFloors(int start, int end, string a)
        {
            for (int i = start; i < end; i++)
            {
                FloorList.Items.Add(createListBoxItem(a + i));
            }
        }

        private void FloorUP_Click(object sender, RoutedEventArgs e)
        {
            if (FloorList.SelectedIndex < 1) return;
            var index = FloorList.SelectedIndex;
            var i = Copy((ListBoxItem)FloorList.Items[index]);
            FloorList.Items.RemoveAt(index);
            index--;
            FloorList.Items.Insert(index, i);
            FloorList.SelectedIndex = index;
            SetTopAndBottomFloor();
        }

        private void FloorDown_Click(object sender, RoutedEventArgs e)
        {
            if (FloorList.SelectedIndex < 0 || FloorList.SelectedIndex > FloorList.Items.Count - 2) return;
            var index = FloorList.SelectedIndex;
            var i = Copy((ListBoxItem)FloorList.Items[index]);
            FloorList.Items.RemoveAt(index);
            index++;
            FloorList.Items.Insert(index, i);
            FloorList.SelectedIndex = index;
            SetTopAndBottomFloor();
        }

        private void SetTopAndBottomFloor()
        {
            if (FloorList.Items.Count == 0) return;
            TopFloor.Text = $"最上階:{GetFloorName(FloorList.Items.Count - 1)}";
            LowFloor.Text = $"最下階:{GetFloorName(0)}";
        }

        private void CreateMiddleFloorItem() => CreateMiddleFloorItem("");

        private void CreateMiddleFloorItem(string FlName)
        {
            var parent = new Grid();
            parent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            parent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60)});
            parent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            var FloorBox = new TextBox();
            FloorBox.Name = "FloorBox";
            FloorBox.Text = FlName;
            FloorBox.MinWidth = 64;
            FloorBox.Margin = new Thickness(8);
            var UpDownSelecter = new Button();
            UpDownSelecter.Name = "UpDownSelecter";
            UpDownSelecter.Margin = new Thickness(8);
            UpDownSelecter.Content = "両";
            UpDownSelecter.Click += (s, e) =>
            {
                switch (UpDownSelecter.Content)
                {
                    case "🔺":
                        UpDownSelecter.Content = "🔻";
                        break;
                    case "🔻":
                        UpDownSelecter.Content = "両";
                        break;
                    case "両":
                        UpDownSelecter.Content = "🔺";
                        break;
                }
            };
            var delete = new Button();
            delete.Name = "Delete";
            delete.Margin = new Thickness(8);
            delete.Content = "削除";
            delete.Click += (s, e) => MiddleFloor.Items.Remove(parent);
            Grid.SetColumn(FloorBox, 0);
            parent.Children.Add(FloorBox);
            Grid.SetColumn(UpDownSelecter, 1);
            parent.Children.Add(UpDownSelecter);
            Grid.SetColumn(delete, 2);
            parent.Children.Add(delete);
            MiddleFloor.Items.Add(parent);
        }

        private void AddMiddleFloor_Click(object sender, RoutedEventArgs e)
        {
            CreateMiddleFloorItem();
        }

        private string[] ConvertFloorListToArray()
        {
            string[] array = new string[FloorList.Items.Count];
            int i = 0;
            foreach(var floor in FloorList.Items)
            {
                array[i++] = ((ListBoxItem)floor).Content as string;
            }
            return array;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (FloorList.Items.Count < 2) return;
            var contentList = createListFromCollection();
            StartButton.IsEnabled = false;
            if (contentList.Where(s => s.requireAuth).Count() != 0)
            {
                try
                {
                    await UAUtil.VerifyTwitterAPI(new TwitterAPIAuthData
                    {
                        AccessToken = AccessToken.Text, AccessTokenSecret = TokenSecret.Text,
                        ConsumerKey = ConsumerKey.Text,ConsumerSecret= ConsumerSecret.Text,
                    });
                } catch
                {
                    StartButton.IsEnabled = true;
                    MessageBox.Show("Twitter APIの認証に失敗しました。");
                    return;
                }

                if (contentList.Where(s => s is RandomTL).Count() > 0)
                {
                    await UAUtil.GetTimeLine(new TwitterAPIAuthData
                    {
                        AccessToken = AccessToken.Text,
                        AccessTokenSecret = TokenSecret.Text,
                        ConsumerKey = ConsumerKey.Text,
                        ConsumerSecret = ConsumerSecret.Text,
                    });
                }
                foreach (var s in contentList.Where(s => s is UserTweet))
                {
                    var us = s as UserTweet;
                    await UAUtil.GetUserTweets(new TwitterAPIAuthData
                    {
                        AccessToken = AccessToken.Text,
                        AccessTokenSecret = TokenSecret.Text,
                        ConsumerKey = ConsumerKey.Text,
                        ConsumerSecret = ConsumerSecret.Text,
                    }, us.UsrName.Text);
                }
            }
            var context = new SimulationContext { AvailableFloors = ConvertFloorListToArray(), 
                RoundTrip =(bool) RoundTrip.IsChecked,Loop = RoundTrip.IsChecked == true ?(bool) Loop.IsChecked : false, startPos = startPos};
            await MainWindow.Instance.DoSimulation(context, StartDelayTime.Value, createListFromCollection());
        }

        private void StartPos_Click(object sender, RoutedEventArgs e)
        {
            startPos = 1 - startPos;
            var a = startPos == 1 ? '下' : '上';
            StartPos.Content = $"開始位置:{a}";
        }

        private void AddInfo_Click(object sender, RoutedEventArgs e) => AddInfo.ContextMenu.IsOpen = true;

        private void CustomNews_Click(object sender, RoutedEventArgs e)=> addContentData(new CustomNews());

        private void addContentData(UABaseContent c)
        {
            DeploySettingUI(c);
            InfoList.Items.Add(c);
        }

        private void DeploySettingUI(UABaseContent content)
        {
            SettingPanel.Children.Clear();
            content.DeploySettingUI(SettingPanel);
        }

        private void InfoList_Selected(object sender, RoutedEventArgs e)
        {
            var i = InfoList.SelectedItem as UABaseContent;
            if (i == null)
            {
                SettingPanel.Children.Clear();
                return;
            }
            DeploySettingUI(i);
            
        }

        private void Media_Click(object sender, RoutedEventArgs e) => addContentData(new Media());

        private void YEmbedContent_Click(object sender, RoutedEventArgs e) => addContentData(new YoutubeEmbed());

        private void RandomTL_Click(object sender, RoutedEventArgs e) => addContentData(new RandomTL());

        private void RandomFromUser_Click(object sender, RoutedEventArgs e) => addContentData(new UserTweet());


        private void DeleteContent_Click(object sender, RoutedEventArgs e)
        {
            int i = InfoList.SelectedIndex;
            if (i == -1) return;
            InfoList.Items.RemoveAt(i);
        }

        private List<UABaseContent> createListFromCollection()
        {
            var ll = InfoList.Items.Cast<UABaseContent>().ToList();

            return ll;
        }

        private async void Verify_Click(object sender, RoutedEventArgs e)
        {
            Verify.IsEnabled = false;
            try
            {
                var auth = new TwitterAPIAuthData
                {
                    AccessToken = AccessToken.Text,
                    AccessTokenSecret = TokenSecret.Text,
                    ConsumerKey = ConsumerKey.Text,
                    ConsumerSecret = ConsumerSecret.Text,
                };
                await UAUtil.VerifyTwitterAPI(auth);
                SaveAuthData();
                UAUtil.AuthData = auth;
                MessageBox.Show("正常に検証できました", "検証完了",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            } catch
            {
                MessageBox.Show("検証に失敗しました。", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Verify.IsEnabled = true;
        }

        string AuthDataPath => Path.Combine(UAUtil.BaseDir, "AuthData.json");

        public void SaveAuthData()
        {
            string json = JsonConvert.SerializeObject(new TwitterAPIAuthData
            {
                ConsumerKey = ConsumerKey.Text,ConsumerSecret= ConsumerSecret.Text,
                AccessToken= AccessToken.Text,AccessTokenSecret= TokenSecret.Text,
            });
            Console.WriteLine(json);
            
            using (var sw = new StreamWriter(AuthDataPath))
            {
                sw.Write(json);
            }
        }

        private void LoadAuthData()
        {
            if (!File.Exists(AuthDataPath)) return;
            using (var sr = new StreamReader(AuthDataPath))
            {
                var authData = JsonConvert.DeserializeObject<TwitterAPIAuthData>(sr.ReadToEnd());
                UAUtil.AuthData = authData;
                ConsumerKey.Text = authData.ConsumerKey;
                ConsumerSecret.Text = authData.ConsumerSecret;
                AccessToken.Text = authData.AccessToken;
                TokenSecret.Text = authData.AccessTokenSecret;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
        }

        private void ImportSettingFromFile(string path)
        {
            FloorList.Items.Clear();
            using (var sr = new StreamReader(path))
            {
                var data = JsonConvert.DeserializeObject<FloorSetting>(sr.ReadToEnd());
                
                foreach (string item in data.Floors)
                {
                    AddSingleFloor(item, true);
                }
                RoundTrip.IsChecked = data.RoundTrip;
                Loop.IsChecked = data.Loop;
            }
        }

        private readonly string SettingFileFilter = "Json Setting File : (*.json) | *.json";

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog= new OpenFileDialog();
            dialog.Title = "インポート";
            dialog.Filter = SettingFileFilter;
            if (dialog.ShowDialog() == true)
                ImportSettingFromFile(dialog.FileName);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "エクスポート";
            dialog.Filter = SettingFileFilter;
            if (dialog.ShowDialog() == true)
            {
                var floorName = FloorList.Items.Cast<ListBoxItem>().Select(s => s.Content as string);
                var data = new FloorSetting
                {
                    RoundTrip =(bool) RoundTrip.IsChecked, Loop=(bool) Loop.IsChecked,
                    Floors = floorName.ToArray()
                };
                string json = JsonConvert.SerializeObject(data);
                using (var sw = new StreamWriter(dialog.FileName))
                {
                    sw.Write(json);
                }
            }
        }
    }
}
