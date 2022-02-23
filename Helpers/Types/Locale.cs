using Nedordle.Game;
using Newtonsoft.Json;

// ReSharper disable UnassignedField.Global
#pragma warning disable CS8618

namespace Nedordle.Helpers.Types;

public class Locale
{
    public static readonly Dictionary<string, Locale> Locales = new();

    #region Game Types

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, GameType> GameTypes = new();

    #endregion

    #region Metrics

    [JsonProperty(PropertyName = "Metrics_ms")]
    public string Milliseconds;

    #endregion

    #region Error Messages

    public string TimedOut;
    public string GuildNotInDatabase;
    public string NotPermitted;

    #endregion

    #region General

    public string Yes;
    public string No;
    public string Random;
    public string PleaseWait;
    public string ApplyingChanges;
    public string Done;
    public string None;
    public string Apply;
    
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
    
    #region Command: Troubleshoot

    [JsonProperty(PropertyName = "Troubleshoot_FindingIssues")]
    public string TroubleshootFindingIssues;
    [JsonProperty(PropertyName = "Troubleshoot_Server")]
    public string TroubleshootServer;
    [JsonProperty(PropertyName = "Troubleshoot_User")]
    public string TroubleshootUser;
    
    [JsonProperty(PropertyName = "Troubleshoot_Fixing")]
    public string TroubleshootFixing;
    [JsonProperty(PropertyName = "Troubleshoot_Done")]
    public string TroubleshootDone;
    [JsonProperty(PropertyName = "Troubleshoot_NoIssues")]
    public string TroubleshootNoIssues;
    
    #endregion
    
    #region Command: Server Settings
    [JsonProperty(PropertyName = "ServerSettings_DMs")]
    public string ServerSettingsDms;
    [JsonProperty(PropertyName = "ServerSettings_AllowCreatingChannels")]
    public string ServerSettingsAllowCreatingChannels;
    [JsonProperty(PropertyName = "ServerSettings_DisallowCreatingChannels")]
    public string ServerSettingsDisallowCreatingChannels;
    [JsonProperty(PropertyName = "ServerSettings_SelectCreateCategory")]
    public string ServerSettingsSelectCreateCategory;
    [JsonProperty(PropertyName = "ServerSettings_Info_AllowCreatingChannels")]
    public string ServerSettingsInfoAllowCreatingChannels;
    [JsonProperty(PropertyName = "ServerSettings_Info_CreateCategory")]
    public string ServerSettingsInfoCreateCategory;
    [JsonProperty(PropertyName = "ServerSettings_Title")]
    public string ServerSettingsTitle;
    [JsonProperty(PropertyName = "ServerSettings_SelectCategory")]
    public string ServerSettingsSelectCategory;
    #endregion
    
    #region Game Type Information
    [JsonProperty(PropertyName = "Game_ChannelInfo")]
    public string GameChannelInfo;
    [JsonProperty(PropertyName = "Game_Start")]
    public string GameStart;
    [JsonProperty(PropertyName = "Game_InvalidLength")]
    public string GameInvalidLength;
    [JsonProperty(PropertyName = "Game_InvalidWord")]
    public string GameInvalidWord;
    [JsonProperty(PropertyName = "Game_Win")]
    public string GameWin;
    [JsonProperty(PropertyName = "Game_Defeat")]
    public string GameDefeat;
    [JsonProperty(PropertyName = "Game_PlayAgain")]
    public string GamePlayAgain;
    [JsonProperty(PropertyName = "Game_AlreadyUsed")]
    public string GameAlreadyUsed;
    #endregion

    #region Locale Information

    public string ShortName;
    public string NativeName;
    public string FullName;
    public string Flag;

    #endregion
}