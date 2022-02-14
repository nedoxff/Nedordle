using Newtonsoft.Json;

// ReSharper disable UnassignedField.Global
#pragma warning disable CS8618

namespace Nedordle.Helpers.Types;

public class Locale
{
    public static readonly Dictionary<string, Locale> Locales = new();
    public string Flag;

    public string FullName;

    [JsonProperty(PropertyName = "Metrics_ms")]
    public string Milliseconds;

    public string NativeName;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public string Ping;

    [JsonProperty(PropertyName = "Ping_AfterMessages")]
    public List<string> PingAfterMessages;

    [JsonProperty(PropertyName = "Ping_BeforeMessages")]
    public List<string> PingBeforeMessages;

    [JsonProperty(PropertyName = "Ping_ClientLatency")]
    public string PingClientLatency;

    [JsonProperty(PropertyName = "Ping_MessageLatency")]
    public string PingMessageLatency;

    [JsonProperty(PropertyName = "SelectLocale_MainText")]
    public string SelectLocaleMainText;

    [JsonProperty(PropertyName = "SelectLocale_Placeholder")]
    public string SelectLocalePlaceholder;

    [JsonProperty(PropertyName = "SelectLocale_Success")]
    public string SelectLocaleSuccess;

    public string ShortName;

    public string TimedOut;
}