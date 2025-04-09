using System;
using UnityEngine;

namespace TestProject.Utils
{
    public static class DeepLinkUtils
    {
        public static bool IsDeepLink(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            url = url.Trim();

            try
            {
                Uri uri = new (url);
                string scheme = uri.Scheme.ToLower();
                return scheme != "http" && scheme != "https";
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static void OpenDeepLink(string url)
        {
            if (!IsDeepLink(url))
                return;

            Application.OpenURL(url);
        }
    }
}
