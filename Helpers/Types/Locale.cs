using Nedordle.Game;
using Newtonsoft.Json;

// ReSharper disable UnassignedField.Global
#pragma warning disable CS8618

namespace Nedordle.Helpers.Types;

public class Locale
{
    public static readonly Dictionary<string, Locale> Locales = new();

    #region Game Types

    public Dictionary<string, GameType> GameTypes;

    #endregion

    #region Metrics

    [JsonProperty(PropertyName = "Metrics_ms")]
    public string Milliseconds;

    #endregion

    #region Error Messages

    public string TimedOut;

    #endregion

    #region General

    public string Yes;
    public string No;
    public string Random;
    public string PleaseWait;

    #endregion

    #region Command: Ping

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

    #endregion

    #region Command: Locale

    [JsonProperty(PropertyName = "SelectLocale_MainText")]
    public string SelectLocaleMainText;

    [JsonProperty(PropertyName = "SelectLocale_Placeholder")]
    public string SelectLocalePlaceholder;

    [JsonProperty(PropertyName = "SelectLocale_Success")]
    public string SelectLocaleSuccess;

    #endregion

    #region Command: Create

    [JsonProperty(PropertyName = "Create_SelectGameType")]
    public string CreateSelectGameType;

    [JsonProperty(PropertyName = "Create_SelectLanguage")]
    public string CreateSelectLanguage;

    [JsonProperty(PropertyName = "Create_SelectLength")]
    public string CreateSelectLength;

    [JsonProperty(PropertyName = "Create_DMs")]
    public string CreateDms;

    [JsonProperty(PropertyName = "Create_Done")]
    public string CreateDone;

    [JsonProperty(PropertyName = "Create_Creating")]
    public string CreateCreating; // :D

    [JsonProperty(PropertyName = "Create_LanguageDescription")]
    public string CreateLanguageDescription;

    #endregion

    #region Locale Information

    public string ShortName;
    public string NativeName;
    public string FullName;
    public string Flag;

    #endregion
}