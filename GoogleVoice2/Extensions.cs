using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVoice2
{
    public class GVLoginException : Exception
    {
        public GVLoginException(string message) : base(message) { }
    }

    public static class Extensions
    {
        public static string FindCookieByName(this CookieContainer c, string Uri, string key)
        {
            foreach (Cookie cookie in c.GetCookies(new Uri(Uri)))
            {
                if (key.ToUpper() == cookie.Name.ToUpper()) return cookie.Value;
            }
            return null;
        }
    }
}
