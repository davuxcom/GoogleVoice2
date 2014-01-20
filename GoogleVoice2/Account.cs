using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoogleVoice2
{
    public class Account
    {
        public class StoredAccountDetails
        {
            public string GVX { get; set; }
            public string RNR_SE { get; set; }
            public int AppVersion { get; set; }

            public bool IsValid()
            {
                return (!string.IsNullOrWhiteSpace(GVX) && !string.IsNullOrWhiteSpace(RNR_SE));
            }
        }

        CookieContainer _Jar = new CookieContainer();

        string _UserName;
        string _Password; // TODO: SecureString
        public StoredAccountDetails _Info; // public for testability

        public Account(string UserName, string Password)
        {
            _UserName = UserName;
            _Password = Password;

            _Info = new StoredAccountDetails();
        }

        public Account(StoredAccountDetails Info)
        {
            _Info = Info;
        }

        HttpClient _GetClient()
        {
            var http = new HttpClient(new HttpClientHandler {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = _Jar,
            });
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (iPhone; U; CPU iPhone OS 7_0 like Mac OS X; en-us) AppleWebKit/532.9 (KHTML, like Gecko) Version/6.0.5 Mobile/8A293 Safari/6531.22.7");
            http.MaxResponseContentBufferSize = int.MaxValue;
            return http;
        }

        public async Task Login()
        {
            var http = _GetClient();
            var ret = await http.GetAsync("https://www.google.com/voice/m");
            if (ret.RequestMessage.RequestUri.LocalPath != "/ServiceLogin")
            {
                throw new GVLoginException("Failed to reach ServiceLogin.");
            }

            var GALX = _Jar.FindCookieByName("https://accounts.google.com/", "GALX");
            if (string.IsNullOrEmpty(GALX))
            {
                throw new GVLoginException("Failed to find GALX cookie.");
            }

            ret = await http.PostAsync("https://accounts.google.com/ServiceLoginAuth?service=grandcentral", 
                new FormUrlEncodedContent(new Dictionary<string, string> {
                            {"ltmpl" , "mobile"},   // Probably don't need one of these
                            {"btmpl" , "mobile"},
                            {"followup", "https://www.google.com/voice/m?initialauth"},
                            {"continue", "https://www.google.com/voice/m?initialauth"},
                            {"service", "grandcentral"},
                            {"bgresponse","js_disabled"},
                            {"PersistentCookie", "yes"},
                            {"GALX", GALX},
                            {"Email", _UserName},
                            {"Passwd", _Password},
                        }));

            if (ret.RequestMessage.RequestUri.ToString() == "https://www.google.com/voice/m")
            {
                // we're logged in!
                _Info.GVX = _Jar.FindCookieByName("https://www.google.com/voice/m", "gvx");
            }
            else if (ret.RequestMessage.RequestUri.ToString().StartsWith("https://accounts.google.com/SmsAuth"))
            {
                // TODO: 2-factor authentication.
            }
            else
            {
                // CheckCookie ... this is a redirect.
                // TEST: Canadian Proxy: 199.185.95.42 8080
                var page = await ret.Content.ReadAsStringAsync();
                Match m = Regex.Match(page, "href=\"(.*?)\"");
                if (m.Success)
                {
                    // NOTE:  this page has URLs that must be HtmlDecoded!
                    var uri = System.Net.WebUtility.HtmlDecode(m.Groups[1].Value);
                    ret = await http.GetAsync("https://www.google.com/voice/m");
                    _Info.GVX = _Jar.FindCookieByName("https://www.google.com/voice/m", "gvx");
                }
                else
                {
                    throw new GVLoginException("Couldn't find continue");
                }
            }

            if (string.IsNullOrEmpty(_Info.GVX))
            {
                throw new GVLoginException("GVX is missing after redirect");
            }

            var Page = await ret.Content.ReadAsStringAsync();
            var m2 = Regex.Match(Page, @"appVersion: (.\d*)", RegexOptions.Singleline);
            if (m2.Success)
            {
                _Info.AppVersion = int.Parse(m2.Groups[1].Value);
            }

            // We're going to fetch the lite mobile page, and pull out an RNR_SE, so we can make calls
            // the HTML5 calling interface is unsuitable for non-phone devices
            http.DefaultRequestHeaders.Clear(); // Clear mobile browser.
            ret = await http.GetAsync("https://www.google.com/voice/m");
            Page = await ret.Content.ReadAsStringAsync();
            Match rnr = Regex.Match(Page, "name=\"_rnr_se\" value=\"(.*?)\"");
            if (rnr.Success)
            {
                // NOTE: we won't encode here, because Post'ing will encode it.
                // but this value MUST be encoded before going out!
                _Info.RNR_SE = rnr.Groups[1].Value;
            }

            if (!_Info.IsValid())
            {
                throw new GVLoginException("Couldn't find login tokens.");
            }
        }

        HttpContent _GetGVPostContent()
        {
            return new StringContent("{\"gvx\":\"" + _Info.GVX + "\"}");
        }

        public async Task PostLoginActivities()
        {
            var http = _GetClient();
            var ret = await http.PostAsync("https://www.google.com/voice/m/x?m=set&v=" + _Info.AppVersion, _GetGVPostContent());
            var content = (await ret.Content.ReadAsStringAsync()).Split('\n')[1];

            var feed = JsonConvert.DeserializeObject<JsonModels.SettingsFeed>(content);

            var x = 1;

        }
    }
}
