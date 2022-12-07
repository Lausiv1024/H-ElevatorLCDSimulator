using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UrbanAce_7.ContentSettings
{
    public class CustomNews : UABaseContent
    {
        [JsonIgnore]
        public TextBoxWithHeader newsText { get; private set; }

        [JsonIgnore]
        public TextBoxWithHeader providedBy { get; private set; }

        public string NewsText { get { return newsText.Text; } set { newsText.Text = value; } }

        public string ProvidedBy { get { return providedBy.Text; } set { providedBy.Text = value; } }

        public CustomNews() : base("CustomNews","カスタムニュース")
        {
            newsText = new TextBoxWithHeader();
            newsText.Header = "メインのテキスト(改行可)";
            providedBy = new TextBoxWithHeader();
            providedBy.Header = "提供元";
            newsText.AcceptReturn = true;
        }

        public override void DeploySettingUI(StackPanel parent)
        {
            parent.Children.Add(newsText);
            parent.Children.Add(providedBy);
        }
    }
}
