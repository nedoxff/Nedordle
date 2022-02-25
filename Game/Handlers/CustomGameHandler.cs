using DSharpPlus;
using DSharpPlus.Entities;
using Nedordle.Core;
using Nedordle.Database;
using Nedordle.Drawer;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;

namespace Nedordle.Game.Handlers;

public class CustomGameHandler : GameHandler
{
    private string _answer = "";
    private DiscordMessage? _responseMessage;
    public int Length { get; init; }

    public override Task OnCreate(DiscordUser creator, Locale locale)
    {
        if (!Channel.IsPrivate)
        {
            var guild = Client.Guilds.First(x => x.Value.Members.Any(y => y.Value.Id == Client.User.Id));
            var permissions =
                Channel.PermissionsFor(guild.Value.Members.First(x => x.Value.Id == Client.User.Id).Value);
            if ((permissions & Permissions.SendMessages) == 0)
                throw new Exception("The bot did not have permissions to write in the channel!");
        }

        _answer = DictionaryDatabaseHelper.GetWord(Language, Length);
        _responseMessage = null;
        Info["Answer"] = _answer;
        Info["ResponseMessage"] = 0;

        Update();
        return Task.CompletedTask;
    }

    public override async Task OnJoined(DiscordChannel callerChannel, DiscordUser user, Locale locale)
    {
        Players[user.Id] = new Player(user, locale);
        Update();
        await OnStart();
    }

    public override async Task OnLeft(DiscordChannel callerChannel, DiscordUser user, Locale locale)
    {
        if (Playing)
        {
            await Channel.SendMessageAsync(locale.CannotLeaveWhilePlaying);
            return;
        }

        await OnEnd();
    }

    public override async Task OnStart()
    {
        await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen,
            Players.First().Value.Locale.GameStart));
    }

    public override async Task OnEnd()
    {
        Ended = true;

        var generated = WordleDrawer.Generate(Players.First().Value.GuessString, Players.First().Value.Theme);
        generated.Seek(0, SeekOrigin.Begin);
        await Channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithFile("result.png", generated));

        await Channel.SendMessageAsync(BuildResult(Players.First().Value));
        await OnCleanup();
    }

    public override async Task OnCleanup()
    {
        if (!Channel.IsPrivate)
        {
            await Channel.SendMessageAsync(Players.First().Value.Locale.GameChannelInfo);
            await Task.Delay(60000);
            await Channel.DeleteAsync();
        }

        GameDatabaseHelper.RemoveGame(Id);
    }

    public override async Task OnInput(DiscordUser user, string input)
    {
        if (Ended) return;

        if (!DictionaryDatabaseHelper.Exists(Language, input))
        {
            var error = await Channel.SendMessageAsync(
                SimpleDiscordEmbed.Error(Players.First().Value.Locale.GameInvalidWord));
            await Task.Delay(5000);
            await error.DeleteAsync();
            return;
        }

        if (Players.First().Value.Guesses.Any(x => x.Input == input))
        {
            var error = await Channel.SendMessageAsync(
                SimpleDiscordEmbed.Error(Players.First().Value.Locale.GameAlreadyUsed));
            await Task.Delay(5000);
            await error.DeleteAsync();
            return;
        }

        if (_responseMessage != null) await _responseMessage.DeleteAsync();
        Players.First().Value.AddGuess(input, _answer);

        if (input == _answer)
        {
            await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen,
                Players.First().Value.Locale.GameWin));
            await OnEnd();
            return;
        }

        if (Players.First().Value.Guesses.Count == 6)
        {
            await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow,
                string.Format(Players.First().Value.Locale.GameDefeat, _answer)));
            await OnEnd();
            return;
        }

        var stream = WordleDrawer.Generate(Players.First().Value.GuessString, Players.First().Value.Theme);
        stream.Seek(0, SeekOrigin.Begin);
        _responseMessage = await Channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithFile($"nedordle_usual_{Id}.png", stream));
        Info["ResponseMessage"] = _responseMessage.Id;
        Update();
    }

    public override string BuildResult(Player player)
    {
        var guessCount = player.Guesses.Count;
        string guessString;
        if (guessCount < 6) guessString = guessCount.ToString();
        else guessString = player.Guesses.Last().CleanOutput.All(x => x == 'c') ? "6" : "X";

        var theme = player.Theme;
        var str = $"Nedordle ({player.Locale.GameTypes[GameType].Name}) {guessString}/6\n\n";
        foreach (var guess in player.Guesses.Select(x => x.CleanOutput))
        {
            foreach (var c in guess)
                switch (c)
                {
                    case 'w':
                        str += theme.WrongEmoji;
                        break;
                    case 'd':
                        str += theme.CloseEmoji;
                        break;
                    case 'c':
                        str += theme.CorrectEmoji;
                        break;
                }

            str += '\n';
        }

        return str;
    }
}