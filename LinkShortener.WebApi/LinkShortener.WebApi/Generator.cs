using System;

namespace LinkShortener.WebApi
{
    //Такой генератор не подошел бы для реального проекта, скорее всего нужно было бы генерировать уникальную ссылку на основе какого-нибудь
    //изменяемого числа, например айдишника в бд.
    //Но я думаю вариант ниже полностью удовлетворит это тестовое задание
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