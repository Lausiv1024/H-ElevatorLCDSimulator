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
    public abstract class UABaseContent
    {
        public string Name { get; set; }

        public string Type { get; set; }

        [JsonIgnore]
        public bool requireAuth { get; protected set; }

        public UABaseContent(string type,string name) 
        { 
            Name = name;
            Type = type;
            requireAuth = false;
        }

        public abstract void DeploySettingUI(StackPanel parent);

        public override string ToString()
        {
            return Name;
        }

        public static bool hasContentRequireTwitterAPI(List<UABaseContent> contents) => contents.Where(c => c.requireAuth).Count() > 0;
    }
}
