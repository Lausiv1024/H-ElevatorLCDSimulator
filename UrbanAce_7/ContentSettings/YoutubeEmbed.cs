using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UrbanAce_7.ContentSettings
{
    internal class YoutubeEmbed : UABaseContent
    {
        [JsonIgnore]
        public TextBoxWithHeader MovID;
        //[JsonIgnore]
        //public NumericBox viewTime;

        public string MovieID { get { return MovID.Text; } set { MovID.Text = value; } }

        public YoutubeEmbed() : base("Youtube","Youtube動画埋め込み")
        {
            MovID= new TextBoxWithHeader();
            MovID.Header = "動画ID";
            //viewTime = new NumericBox();
        }

        public override void DeploySettingUI(StackPanel parent)
        {
            parent.Children.Add(MovID);
            //var b  = new TextBlock();
            //b.Text = "持続時間(秒  0の場合デフォルト値)";
            //parent.Children.Add(b);
            //parent.Children.Add(viewTime);
        }
    }
}
