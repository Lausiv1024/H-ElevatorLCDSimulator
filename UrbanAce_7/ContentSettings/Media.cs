using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UrbanAce_7.ContentSettings
{
    internal class Media : UABaseContent
    {
        public string MediaPath { get { return Path.Text; } set { Path.Text = value; } }

        [JsonIgnore]
        public TextBoxWithHeader Path { get; private set; }

        public Media() : base("Media","メディア")
        {
            Path = new TextBoxWithHeader();
            Path.Header = "メディアへのパス";
        }

        public override void DeploySettingUI(StackPanel parent)
        {
            parent.Children.Add(Path);
        }
    }
}
