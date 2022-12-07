using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UrbanAce_7.ContentSettings
{
    internal class UserTweet : UABaseContent
    {
        [JsonIgnore]
        public TextBoxWithHeader UsrName { get; set; }

        public string UserName { get { return UsrName.Text; } set { UsrName.Text = value; } }
        public UserTweet() : base("UserTweet","ユーザーのツイート")
        {
            requireAuth = true;
            UsrName = new TextBoxWithHeader { Header = "ユーザーID" };
        }

        public override void DeploySettingUI(StackPanel parent)
        {
            parent.Children.Add(UsrName);
        }

        
    }
}
