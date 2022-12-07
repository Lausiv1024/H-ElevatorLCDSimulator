using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UrbanAce_7.ContentSettings
{
    internal class RandomTL : UABaseContent
    {
        public RandomTL() : base("RandomTL","タイムライン")
        {
            requireAuth= true;
        }

        public override void DeploySettingUI(StackPanel parent)
        {
            TextBlock textBlock = new TextBlock { Text = "設定項目はありません。" };
            parent.Children.Add(textBlock);
        }
    }
}
