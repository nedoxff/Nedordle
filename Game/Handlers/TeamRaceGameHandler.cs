using DSharpPlus;
using DSharpPlus.Entities;
using Nedordle.Database;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;

namespace Nedordle.Game.Handlers;

public class TeamRaceGameHandler : GameHandler
{
    private readonly Dictionary<ulong, ulong> _channelIds = new();

    private readonly Dictionary<ulong, DiscordChannel> _channels = new();
    private DiscordMessage _createMessage;
    public int UserLimit { get; init; }
    public int Length { get; init; }

    public override async Task OnCreate(DiscordUser creator, Locale locale)
    {
        Info["Channels"] = _channelIds;
        Info["Playing"] = Playing;

        _createMessage =
            await Channel.SendMessageAsync(
                SimpleDiscordEmbed.Warning(string.Format(locale.GameMultiplayerInfo, UserLimit)));
        Update();
    }

    public override async Task OnJoined(DiscordChannel callerChannel, DiscordUser user, Locale locale)
    {
        if (Players.Count == UserLimit)
        {
            await callerChannel.SendMessageAsync(SimpleDiscordEmbed.Error(locale.GameMultiplayerFull));
            return;
        }

        if (Playing)
        {
            await callerChannel.SendMessageAsync(SimpleDiscordEmbed.Error(locale.AlreadyStarted));
            return;
        }

        Players[user.Id] = new Player(user, locale);
        _channels[user.Id] = Channel.IsPrivate ? callerChannel : Channel;
        _channelIds[user.Id] = Channel.IsPrivate ? callerChannel.Id : Channel.Id;

        if (!Channel.IsPrivate)
        {
            await _createMessage.ModifyAsync(x =>
                x.AddEmbed(SimpleDiscordEmbed.Warning(CombineLocales("GameMultiplayerInfo", UserLimit))));
            await Channel.AddOverwriteAsync(await Channel.Guild.GetMemberAsync(user.Id),
                Permissions.SendMessages | Permissions.AccessChannels | Permissions.ReadMessageHistory,
                Permissions.None, $"User joined ({user.Id})");
        }
        else
        {
            await callerChannel.SendMessageAsync(
                SimpleDiscordEmbed.Warning(string.Format(locale.GameMultiplayerInfo, UserLimit)));
        }

        Update();
        await BroadcastSuccess("GameMultiplayerJoined", user.Username, Players.Count, UserLimit);


        if (Players.Count == UserLimit)
            await OnStart();
    }

    public override async Task OnLeft(DiscordChannel callerChannel, DiscordUser user, Locale locale)
    {
        if (Playing)
        {
            await callerChannel.SendMessageAsync(locale.CannotLeaveWhilePlaying);
            return;
        }

        _channels.Remove(user.Id);
        _channelIds.Remove(user.Id);
        Players.Remove(user.Id);

        if (!Channel.IsPrivate)
            await Channel.AddOverwriteAsync(await Channel.Guild.GetMemberAsync(user.Id), Permissions.None,
                Permissions.All, $"User left ({user.Id})");

        Update();
        await BroadcastError("GameMultiplayerLeft", user.Username, Players.Count, UserLimit);

        if (Players.Count == 0)
            await OnCleanup();
    }

    public override async Task OnStart()
    {
    }

    public override async Task OnEnd()
    {
    }

    public override async Task OnCleanup()
    {
        if (!Channel.IsPrivate)
        {
            await Channel.SendMessageAsync(Players[0].Locale.GameChannelInfo);
            await Task.Delay(60000);
            await Channel.DeleteAsync();
        }

        GameDatabaseHelper.RemoveGame(Id);
    }

    public override async Task OnInput(DiscordUser user, string input)
    {
    }

    public override string BuildResult(Player player)
    {
        return "";
    }

    private Task BroadcastWarning(string propertyName, params object[] objs)
    {
        return Broadcast(SimpleDiscordEmbed.Warning(), propertyName, objs);
    }

    private Task BroadcastError(string propertyName, params object[] objs)
    {
        return Broadcast(SimpleDiscordEmbed.Error(), propertyName, objs);
    }

    private Task BroadcastSuccess(string propertyName, params object[] objs)
    {
        return Broadcast(SimpleDiscordEmbed.Success(), propertyName, objs);
    }

    private async Task Broadcast(DiscordEmbedBuilder builder, string propertyName, params object[] objs)
    {
        if (Channel.IsPrivate)
            foreach (var c in _channels)
                await c.Value.SendMessageAsync(builder.WithDescription(GetLocaleProperty(c.Key, propertyName, objs)));
        else
            await Channel.SendMessageAsync(builder.WithDescription(CombineLocales(propertyName, objs)));
    }

    private string CombineLocales(string propertyName, params object[] objs)
    {
        var list = new List<Locale>();
        foreach (var p in Players.Where(p => !list.Contains(p.Value.Locale)))
            list.Add(p.Value.Locale);

        return list.Aggregate("\n", (current, l) => current + GetLocaleProperty(l, propertyName, objs) + "\n");
    }

    private string GetLocaleProperty(ulong id, string propertyName, params object[] objs)
    {
        var locale = Players[id].Locale;
        var str =
            (string) ((typeof(Locale).GetField(propertyName) ?? throw new InvalidOperationException())
                .GetValue(locale) ?? throw new InvalidOperationException());
        return objs.Length != 0 ? string.Format(str, objs) : str;
    }

    private string GetLocaleProperty(Locale locale, string propertyName, params object[] objs)
    {
        var str =
            (string) ((typeof(Locale).GetField(propertyName) ?? throw new InvalidOperationException())
                .GetValue(locale) ?? throw new InvalidOperationException());
        return objs.Length != 0 ? string.Format(str, objs) : str;
    }
}