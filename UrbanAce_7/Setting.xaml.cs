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
using System.Text.RegularExpressions;

namespace UrbanAce_7
{
    /// <summary>
    /// Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Setting : Page
    {
        public Setting()
        {
            InitializeComponent();
            FloorS.PreviewTextInput += (s, e) =>
            {
                e.Handled = (FloorS.Text.Length + e.Text.Length) > 3;
            };
            addEnterAction(FloorS, () => AddSingleFloor());
            foreach (int s in UAUtil.Speeds) Speed.Items.Add(s);
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

        private void AddFloor_Click(object sender, RoutedEventArgs e) => AddSingleFloor();
        private void AddSingleFloor()
        {
            if (FloorS.Text.Length == 0 || CheckAlreadyExists(FloorS.Text)) return;
            if (FloorList.SelectedIndex == -1)
                FloorList.Items.Add(createListBoxItem(FloorS.Text));
            else FloorList.Items.Insert(FloorList.SelectedIndex, createListBoxItem(FloorS.Text));
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
                CreateMiddleFloorItem(getFloorName(FloorList.SelectedIndex));
            };

            i.ContextMenu.Items.Add(del);
            i.ContextMenu.Items.Add(addToMiddlefloorList);
            return i;
        }

        private ListBoxItem copy(ListBoxItem item) => createListBoxItem((string)item.Content);

        private bool CheckAlreadyExists(string Name)
        {
            foreach (var item in FloorList.Items)
            {
                var listboxItem = item as ListBoxItem;
                if ((string)listboxItem.Content == Name) return true;
            }
            return false;
        }

        private string getFloorName(int index)
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
            var i = copy((ListBoxItem)FloorList.Items[index]);
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
            var i = copy((ListBoxItem)FloorList.Items[index]);
            FloorList.Items.RemoveAt(index);
            index++;
            FloorList.Items.Insert(index, i);
            FloorList.SelectedIndex = index;
            SetTopAndBottomFloor();
        }

        private void SetTopAndBottomFloor()
        {
            if (FloorList.Items.Count == 0) return;
            TopFloor.Text = $"最上階:{getFloorName(FloorList.Items.Count - 1)}";
            LowFloor.Text = $"最下層:{getFloorName(0)}";
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
    }
}
