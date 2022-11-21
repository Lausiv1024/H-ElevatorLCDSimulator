using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanAce_7
{
    public class IntroAndWarn
    {
        public static string[] WARNS = new string[] {"ドアが閉じ始めた\nときは、乗り込ま\nないでください。","かご内で、飛んだり\n跳ねたりしないで\nください。",
        "ドアに手を触れてい\nたりドアをこじ開け\nないでください。","ひもやコードがドア\nに挟まれないように\nしてください。"};
        public static string[] INTRODUCTION = new string[] {"乗り降りする人を検知\nすると戸を反転します。",
        "地震により運転休止した\n場合自動診断を実施し異常\nがなければ仮復旧します。","誤って押した行先階ボタン\nは、停止中に二度押しする\nことで取り消せます。",
        "地震を感知すると\n自動的に最寄り階で\n停止してドアを開きます。"};
    }
}
