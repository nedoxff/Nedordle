using Newtonsoft.Json;

namespace Nedordle.Core.Types;

public class Locale
{
    public static Dictionary<string, Locale> Locales = new();

    [JsonProperty(PropertyName = "Ping_BeforeMessages")]
    public List<string> PingBeforeMessages;
    
    [JsonProperty(PropertyName = "Ping_AfterMessages")]
    public List<string> PingAfterMessages;

    public string Ping;
    
    [JsonProperty(PropertyName = "Ping_ClientLatency")]
    public string PingClientLatency;
    
    [JsonProperty(PropertyName = "Ping_MessageLatency")]
    public string PingMessageLatency;

    [JsonProperty(PropertyName = "Metrics_ms")]
    public string Milliseconds;
}