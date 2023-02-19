using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tweetinvi;
using Tweetinvi.Models;

namespace UrbanAce_7
{
    public class UAUtil
    {
        public static string ResourceDirectoryPath => $"{BaseDir}\\resources\\";

        public static Random Rand = new Random();
        public static Uri RandomWarning => new Uri($@"pack://application:,,,/Resources/Warn{Rand.Next(1,5)}.png");

        public static Uri RandomIntro => new Uri($@"pack://application:,,,/Resources/Intro{Rand.Next(1, 6)}.png");

        public static string BaseDir => Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

        public static TwitterAPIAuthData AuthData { get; set; }

        public static TwitterClient GetClient(TwitterAPIAuthData authData) => new TwitterClient(authData.ConsumerKey, authData.ConsumerSecret,
            authData.AccessToken, authData.AccessTokenSecret);

        public static TwitterClient GetClient() => GetClient(AuthData);

        public static bool TwitterVerified = false;

        public static ITweet[] TLTweets = new ITweet[0];
        public static List<ITweet> UserTweets = new List<ITweet>();

        public static int LoopCount = 0;

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

        public static async Task VerifyTwitterAPI(TwitterAPIAuthData auth)
        {
            var client = new TwitterClient(auth.ConsumerKey, auth.ConsumerSecret,
                auth.AccessToken, auth.AccessTokenSecret);
            await client.Users.GetAuthenticatedUserAsync();
            
        }

        public static async Task GetUserTweets(TwitterAPIAuthData auth, string userId)
        {
            var client = new TwitterClient(auth.ConsumerKey, auth.ConsumerSecret,
                auth.AccessToken, auth.AccessTokenSecret);
            var tl = await client.Timelines.GetUserTimelineAsync(userId);
            UserTweets.AddRange(tl.Where(s => s.Media.Count == 0).Where(t => !t.FullText.Contains("@"))
                .Where(t => t.FullText.Count(r => r == '\n') < 5)
                .Where(s => !s.FullText.Contains("https://"))
                .Where(s => !s.FullText.Contains("http://")).Where(t => t.FullText.Length < 90));
        }

        public static async Task GetTimeLine(TwitterAPIAuthData auth)
        {
            var client = new TwitterClient(auth.ConsumerKey, auth.ConsumerSecret,
                auth.AccessToken, auth.AccessTokenSecret);
            var tl = await client.Timelines.GetHomeTimelineAsync();
            TLTweets = tl.Where(s => !s.IsRetweet).Where(s => s.Media.Count == 0).Where(s => !s.FullText.Contains("https://"))
                .Where(t => !t.FullText.Contains("@")).Where(t => t.FullText.Count(r => r == '\n') < 5).Where(t => t.FullText.Length < 90)
                .ToArray();
        }
    }
}
