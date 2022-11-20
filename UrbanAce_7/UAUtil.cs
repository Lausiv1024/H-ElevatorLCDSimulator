using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanAce_7
{
    public class UAUtil
    {

        public static string Ordinal(int i)
        {
            if (i / 10 != 1)
            {
                if (i % 10 == 1) return "st";
                if (i % 10 == 2) return "nd";
                if (i % 10 == 3) return "rd";
            }
            return "th";
        }
        public static string ToOrdinalNum(int i) => i + Ordinal(i);

        public static int[] Speeds = new int[] {60, 150,240,300};

        public enum FadeType
        {
            IN,
            OUT
        }

        public static void PlayFloorArriveSound(string floorName)
        {

        }
    }
}
