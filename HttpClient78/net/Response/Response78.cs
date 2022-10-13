using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace www778878net.Net.Response
{
    /// <summary>
    /// 7788API标准返回
    /// </summary>
    public class Response78
    {
        [JsonProperty("res")]
        public int Res { get; set; }

        [JsonProperty("back")]
        public object? Back { get; set; }

        [JsonProperty("errmsg")]
        public string? Errmsg { get; set; }

        [JsonProperty("kind")]
        public string? Kind { get; set; }
    }
    /// <summary>
    /// 确认返回JTOKEN
    /// </summary>
    public class ResponseJToken78
    {
        [JsonProperty("res")]
        public int Res { get; set; }

        [JsonProperty("back")]
        public JToken? Back { get; set; }

        [JsonProperty("errmsg")]
        public string? Errmsg { get; set; }

        [JsonProperty("kind")]
        public string? Kind { get; set; }
    }
}
