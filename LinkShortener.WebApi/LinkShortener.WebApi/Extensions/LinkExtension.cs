using System;

namespace LinkShortener.WebApi.Extensions
{
    public static class LinkExtension
    {
        public static bool IsValidLink(string linkString) => Uri.TryCreate(linkString, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}