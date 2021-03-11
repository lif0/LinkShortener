using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LinkShortener.WebApi
{
    public static class Global
    {
        public static string BrowserUserId = "BrowserUserId";
        public static ConcurrentDictionary<string, List<string>> NoAuthUsersDict = new ConcurrentDictionary<string, List<string>>();
    }
}