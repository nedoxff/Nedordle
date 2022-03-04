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

    #region Theme Information

    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public Dictionary<string, string> Themes = new();

    #endregion

    #region Error Messages

    public string TimedOut;
    public string NotPermitted;
    public string CannotLeaveWhilePlaying;
    public string UserInGame;
    public string NotSupported;
    public string GameNotFound;
    public string AlreadyStarted;

    #endregion

    #region General

    public string Yes;
    public string No;
    public string Fixing;
    public string Random;
    public string PleaseWait;
    public string ApplyingChanges;
    public string Done;
    public string None;
    public string Apply;
    public string Success;
    public string Custom;
    public string Unlimited;

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

    #region Command: Create

    [JsonProperty(PropertyName = "Create_SelectGameType")]
    public string CreateSelectGameType;

    [JsonProperty(PropertyName = "Create_SelectLanguage")]
    public string CreateSelectLanguage;

    [JsonProperty(PropertyName = "Create_SelectLength")]
    public string CreateSelectLength;

    [JsonProperty(PropertyName = "Create_LengthDescription")]
    public string CreateLengthDescription;

    [JsonProperty(PropertyName = "Create_DMs")]
    public string CreateDms;

    [JsonProperty(PropertyName = "Create_DoneMultiplayer")]
    public string CreateDoneMultiplayer;

    [JsonProperty(PropertyName = "Create_Creating")]
    public string CreateCreating; // :D

    [JsonProperty(PropertyName = "Create_LanguageDescription")]
    public string CreateLanguageDescription;

    [JsonProperty(PropertyName = "Create_SelectUserLimit")]
    public string CreateSelectUserLimit;

    [JsonProperty(PropertyName = "Create_SelectAttemptLimit")]
    public string CreateSelectAttemptLimit;
    
    [JsonProperty(PropertyName = "Create_EnterAttemptLimit")]
    public string CreateEnterAttemptLimit;

    #endregion

    #region Command: Server Settings

    [JsonProperty(PropertyName = "ServerSettings_DMs")]
    public string ServerSettingsDms;

    [JsonProperty(PropertyName = "ServerSettings_AllowCreatingChannels")]
    public string ServerSettingsAllowCreatingChannels;

    [JsonProperty(PropertyName = "ServerSettings_DisallowCreatingChannels")]
    public string ServerSettingsDisallowCreatingChannels;

    [JsonProperty(PropertyName = "ServerSettings_ChangeCreateCategory")]
    public string ServerSettingsChangeCreateCategory;

    [JsonProperty(PropertyName = "ServerSettings_Info_AllowCreatingChannels")]
    public string ServerSettingsInfoAllowCreatingChannels;

    [JsonProperty(PropertyName = "ServerSettings_Info_CreateCategory")]
    public string ServerSettingsInfoCreateCategory;

    [JsonProperty(PropertyName = "ServerSettings_Title")]
    public string ServerSettingsTitle;

    [JsonProperty(PropertyName = "ServerSettings_SelectCreateCategoryPlaceholder")]
    public string ServerSettingsSelectCreateCategoryPlaceholder;

    #endregion

    #region Command: User Settings

    [JsonProperty(PropertyName = "UserSettings_Title")]
    public string UserSettingsTitle;

    [JsonProperty(PropertyName = "UserSettings_Theme")]
    public string UserSettingsTheme;

    [JsonProperty(PropertyName = "UserSettings_ChangeTheme")]
    public string UserSettingsChangeTheme;

    [JsonProperty(PropertyName = "UserSettings_SelectThemePlaceholder")]
    public string UserSettingsSelectThemePlaceholder;

    #endregion

    #region Game Information

    [JsonProperty(PropertyName = "Game_ChannelInfo")]
    public string GameChannelInfo;

    [JsonProperty(PropertyName = "Game_Start")]
    public string GameStart;
    
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

    #region Game (multiplayer) Information

    [JsonProperty(PropertyName = "Game_MultiplayerInfo")]
    public string GameMultiplayerInfo;

    [JsonProperty(PropertyName = "Game_MultiplayerFull")]
    public string GameMultiplayerFull;

    [JsonProperty(PropertyName = "Game_MultiplayerJoined")]
    public string GameMultiplayerJoined;

    [JsonProperty(PropertyName = "Game_MultiplayerLeft")]
    public string GameMultiplayerLeft;

    [JsonProperty(PropertyName = "Game_MultiplayerWinner")]
    public string GameMultiplayerWinner;

    #endregion
    
    #region Game Type: teamwork
    [JsonProperty(PropertyName = "Game_Teamwork_Start")]
    public string GameTeamworkStart;
    [JsonProperty(PropertyName = "Game_Teamwork_YourTurn")]
    public string GameTeamworkYourTurn;
    [JsonProperty(PropertyName = "Game_Teamwork_NotYourTurn")]
    public string GameTeamworkNotYourTurn;
    [JsonProperty(PropertyName = "Game_Teamwork_AnswerSent")]
    public string GameTeamworkAnswerSent;
    [JsonProperty(PropertyName = "Game_Teamwork_Win")]
    public string GameTeamworkWin;
    [JsonProperty(PropertyName = "Game_Teamwork_Defeat")]
    public string GameTeamworkDefeat;
    #endregion

    #region Locale Information

    public string ShortName;
    public string NativeName;
    public string FullName;
    public string Flag;

    #endregion
}