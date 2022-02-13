using Newtonsoft.Json;

namespace Nedordle.Helpers.Types;

public class Locale
{
    public static readonly Dictionary<string, Locale> Locales = new();

    [JsonProperty(PropertyName = "Metrics_ms")]
    public string Milliseconds = null!;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public string Ping = null!;

    [JsonProperty(PropertyName = "Ping_AfterMessages")]
    public List<string> PingAfterMessages = null!;

    [JsonProperty(PropertyName = "Ping_BeforeMessages")]
    public List<string> PingBeforeMessages = null!;

    [JsonProperty(PropertyName = "Ping_ClientLatency")]
    public string PingClientLatency = null!;

    [JsonProperty(PropertyName = "Ping_MessageLatency")]
    public string PingMessageLatency = null!;

    public string FullName = null!;
    public string ShortName = null!;
    public string NativeName = null!;
    public string Flag = null!;
}