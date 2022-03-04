using System.Linq;
using DSharpPlus.Entities;
using Nedordle.Database;
using Nedordle.Drawer;
using Nedordle.Helpers;
using Nedordle.Helpers.Game;
using Nedordle.Helpers.Types;

namespace Nedordle.Game.Handlers;

public class TeamworkGameHandler: MultiplayerGameHandler
{

    public int UserLimit { get; init; }
    public int Length { get; init; }
    public int AttemptLimit { get; init; }
    private string _answer = "";
    private int _current;
    public List<Guess> Guesses { get; } = new();
    

    public override Task OnCreate(DiscordUser creator, Locale locale)
    {
        UpdateInfo();
        Update();
        return Task.CompletedTask;
    }

    private protected override void UpdateInfo()
    {
        Info["Channels"] = Channels.ToDictionary(x => x.Key, x => x.Value.Id);
        Info["ResponseMessages"] = ResponseMessages.ToDictionary(x => x.Key, x => x.Value == null ? 0: x.Value.Id);
        Info["Playing"] = Playing;
        Info["Answer"] = _answer;
        Info["Current"] = _current;
        Info["Guesses"] = Guesses;
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
        var channel =  await MultiplayerHelper.GetPrivateChannel(user, callerChannel);
        await channel.SendMessageAsync(string.Format(locale.GameMultiplayerInfo, UserLimit));
        Channels[user.Id] = channel;

        UpdateInfo(); 
        Update();
        
        await ChannelBroadcaster.Broadcast(this, ChannelBroadcaster.BroadcastLevel.Success, "GameMultiplayerJoined", user.Username, Players.Count, UserLimit);
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

        Channels.Remove(user.Id);
        Players.Remove(user.Id);

        UpdateInfo();
        Update();

        await ChannelBroadcaster.Broadcast(this, ChannelBroadcaster.BroadcastLevel.Error, "GameMultiplayerLeft", user.Username, Players.Count, UserLimit);

        if (Players.Count == 0)
            await OnCleanup();
    }

    public override async Task OnStart()
    {
        Playing = true;
        
        _answer = DictionaryDatabaseHelper.GetWord(Language, Length);
        UpdateInfo();
        Update();
        await ChannelBroadcaster.Broadcast(this, ChannelBroadcaster.BroadcastLevel.Success, "GameTeamworkStart");

        var start = RandomExtensions.New().NextElement(Players);
        _current = Players.ToList().IndexOf(start);
        await Channels[start.Key].SendMessageAsync(SimpleDiscordEmbed.Success(start.Value.Locale.GameTeamworkYourTurn));
    }

    public override async Task OnEnd()
    {
        Ended = true;
        
        foreach (var (_, value) in Players)
            await Channels[value.User.Id].SendMessageAsync(BuildResult(value));

        await OnCleanup();
    }

    public override Task OnCleanup()
    {
        GameDatabaseHelper.RemoveGame(Id);
        return Task.CompletedTask;
    }

    public override async Task OnInput(DiscordChannel caller, DiscordUser user, string input)
    {
        if (Ended) return;

        if (caller.Id != Channels[user.Id].Id) return;
        
        if (input.Length != Length) return;

        var playerIndex = IndexOfPlayer(user.Id);
        if (playerIndex != _current)
        {
            await caller.SendMessageAsync(SimpleDiscordEmbed.Error(Players[user.Id].Locale.GameTeamworkNotYourTurn));
            return;
        }

        if (!DictionaryDatabaseHelper.Exists(Language, input))
        {
            var error = await Channels[user.Id].SendMessageAsync(
                SimpleDiscordEmbed.Error(Players[user.Id].Locale.GameInvalidWord));
            await Task.Delay(5000);
            await error.DeleteAsync();
            return;
        }

        var guesses = Players.Select(x => x.Value.Guesses).SelectMany(x => x);
        if (guesses.Any(x => x.Input == input))
        {
            var error = await Channels[user.Id].SendMessageAsync(
                SimpleDiscordEmbed.Error(Players[user.Id].Locale.GameAlreadyUsed));
            await Task.Delay(5000);
            await error.DeleteAsync();
            return;
        }
        
        var (formatted, clean) = WordleStringComparer.Compare(input, _answer);
        Guesses.Add(new Guess
        {
            Input = input,
            CleanOutput = clean,
            FormattedOutput = formatted
        });
        
        _current++;
        if (_current >= Players.Count) _current = 0;
        var nextUser = Players.Values.ToList()[_current];

        var stream = WordleDrawer.Generate(string.Join('\n', Guesses.Select(x => x.FormattedOutput)), Players[nextUser.UserId].Theme);
        stream.Seek(0, SeekOrigin.Begin);
        
        if (ResponseMessages.ContainsKey(user.Id) && ResponseMessages[user.Id] != null) await ResponseMessages[user.Id]!.DeleteAsync();
        ResponseMessages[user.Id] = await Channels[user.Id].SendMessageAsync(new DiscordMessageBuilder()
            .WithEmbed(SimpleDiscordEmbed.Success(Players[user.Id].Locale.GameTeamworkAnswerSent)));
        
        if (input == _answer)
        {
            await ChannelBroadcaster.Broadcast(this, ChannelBroadcaster.BroadcastLevel.Success,
                "GameTeamworkWin");
            await OnEnd();
            return;
        }
        
        if (Guesses.Count == AttemptLimit)
        {
            await ChannelBroadcaster.Broadcast(this, ChannelBroadcaster.BroadcastLevel.Error,
                "GameTeamworkDefeat", _answer);
            await OnEnd();
            return;
        }
        
        if (ResponseMessages.ContainsKey(nextUser.UserId) && ResponseMessages[nextUser.UserId] != null) await ResponseMessages[nextUser.UserId]!.DeleteAsync();
        ResponseMessages[nextUser.UserId] = await Channels[nextUser.UserId].SendMessageAsync(new DiscordMessageBuilder()
            .WithEmbed(SimpleDiscordEmbed.Success(nextUser.Locale.GameTeamworkYourTurn))
            .WithFile($"nedordle_teamwork_{Id}.png", stream));
        
        UpdateInfo();
        Update();
    }
    
    public override string BuildResult(Player player) => EmojiResultBuilder.BuildTeamWork(this, player);

    private int IndexOfPlayer(ulong id) => Players.Keys.ToList().IndexOf(id);
}