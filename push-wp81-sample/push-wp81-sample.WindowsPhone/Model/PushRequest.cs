using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace push_wp81_sample.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    class PushRequest
    {
        [DataMember, JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public PushRequestMessage Message { get; set; }

        [DataMember, JsonProperty(PropertyName = "target", NullValueHandling = NullValueHandling.Ignore)]
        public PushRequestTarget Target { get; set; }

        private PushRequest()
        {
        }

        public static PushRequest MakePushRequest(string body, string[] devices)
        {
            return new PushRequest
            {
                Message = new PushRequestMessage
                {
                    Body = body
                },

                Target = new PushRequestTarget
                {
                    Platform = "windows8",
                    Devices = devices
                }
            };
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    internal class PushRequestMessage
    {
        [DataMember, JsonProperty(PropertyName = "body", NullValueHandling = NullValueHandling.Ignore)]
        public string Body { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    internal class PushRequestTarget
    {
        [DataMember, JsonProperty(PropertyName = "platform", NullValueHandling = NullValueHandling.Ignore)]
        public string Platform { get; set; }
     
        [DataMember, JsonProperty(PropertyName = "devices", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Devices { get; set; }
    }
}
