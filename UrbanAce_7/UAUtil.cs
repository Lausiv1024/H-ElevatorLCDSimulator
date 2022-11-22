using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanAce_7
{
    public class UAUtil
    {
        public static string ResourceDirectoryPath => $"{Directory.GetCurrentDirectory()}\\resources\\";

        public static Random Rand = new Random();
        public static Uri RandomWarning => new Uri($@"pack://application:,,,/Resources/Warn{Rand.Next(1,5)}.png");

        public static Uri RandomIntro => new Uri($@"pack://application:,,,/Resources/Intro{Rand.Next(1, 6)}.png");

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

        public static string GetUsDayOfWeek(DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Sunday:
                    return "SUN.";
                case DayOfWeek.Monday:
                    return "MON.";
                case DayOfWeek.Tuesday:
                    return "TUE.";
                case DayOfWeek.Wednesday:
                    return "WED.";
                case DayOfWeek.Thursday:
                    return "THU.";
                case DayOfWeek.Friday:
                    return "FRI.";
                case DayOfWeek.Saturday:
                    return "SAT.";
            }
            return string.Empty;
        }
    }
}
