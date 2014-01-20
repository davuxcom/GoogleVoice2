using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoogleVoice2.JsonModels
{
    [DataContract]
    class SettingsFeed
    {
        [DataMember(Name = "rnr_xsrf_token")]
        public string rnr_xsrf_token { get; set; }

        [DataMember(Name = "standard_email")]
        public string Email { get; set; }

        [DataMember(Name = "soi_status")]
        public bool IsSOI { get; set; }

        [DataMember(Name = "app_version")]
        public bool AppVersion { get; set; }



    }
}
