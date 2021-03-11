using System;
using System.Linq;
using System.Text;

namespace LinkShortener.WebApi
{
    public static class Generator
    {
        private static Random _random = new Random();
        private const string _chars = "abcdefghjklmnopqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXY";
        private static readonly object ThreadLock = new object();
        
        public static string GenerateUniqLink()
        {
            char[] link = new char[8];
            for (var i = 0; i < 8; i++)
            {
                lock (ThreadLock)
                {
                    var charIndex = _random.Next(0, _chars.Length);
                    link[i] = _chars[charIndex];
                }
            }
            return new string(link);
        }
    }
}