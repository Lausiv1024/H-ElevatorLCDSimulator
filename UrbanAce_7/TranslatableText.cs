using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanAce_7
{
    public class TranslatableInfoText
    {
        public static TranslatableInfoText DoorClose = new TranslatableInfoText("ドアがしまります", "Door Closing", InfoType.DOOR);
        public static TranslatableInfoText DoorOpen = new TranslatableInfoText("ドアがひらきます", "Door Opening", InfoType.DOOR);
        public static TranslatableInfoText NextFloor = new TranslatableInfoText("つぎは      階です", "Next             floor", InfoType.FLOOR);
        public static TranslatableInfoText Empty = new TranslatableInfoText("", "", InfoType.NONE);

        public readonly string JP;
        public readonly string US;
        public readonly InfoType infoType;

        public TranslatableInfoText(string jP, string uS, InfoType infoType)
        {
            JP = jP;
            US = uS;
            this.infoType = infoType;
        }

        public enum InfoType { 
            NONE,
            FLOOR,
            DOOR
        }
    }
}
