using DSharpPlus;
using DSharpPlus.Entities;
using Nedordle.Core;
using Nedordle.Database;
using Nedordle.Drawer;
using Nedordle.Helpers;
using Serilog;

namespace Nedordle.Game.Handlers;

public class RegularGameHandler : GameHandler
{
    private int _guess;
    private bool _acceptInput = true;
    private DiscordMessage? _lastResponseMessage;
    
    public override async Task OnCreate()
    {
        if (!Channel.IsPrivate)
        {
            var guild = Client.Guilds.First(x => x.Value.Members.Any(y => y.Value.Id == Client.User.Id));
            var permissions = Channel.PermissionsFor(guild.Value.Members.First(x => x.Value.Id == Client.User.Id).Value);
            if ((permissions & Permissions.SendMessages) == 0)
                throw new Exception("The bot did not have permissions to write in the channel!");
        }

        GameType = "usual";
        Info["ANSWER"] = DictionaryDatabaseHelper.GetWord(Language, 5).ToLower();
        Info["GUESSES"] = new List<string>();
        Info["GUESSES_RAW"] = new List<string>();
        Info["GUESSES_LETTERS"] = new List<string>();
        Info["RESPONSE_MESSAGE"] = 0;

        Update();
    }
    
    public override async Task OnJoined(DiscordUser user)
    {
        if (Players.Count == 0)
        {
            Players.Add(new Player
            {
                UserId = user.Id,
                User = user
            });
            Update();   
        }

        await OnStart();
    }

    public override async Task OnLeft(DiscordUser user)
    {
        if (Players.Count == 1) await OnEnd();
    }

    public override async Task OnStart() => await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, Locale.GameStart));

    public override async Task OnEnd()
    {
        _acceptInput = false;
        var str = ((List<string>) Info["GUESSES"]).Aggregate("", (current, guess) => current + guess + "\n");
        str = str[..str.LastIndexOf("\n", StringComparison.Ordinal)];
        var generated = WordleDrawer.Generate(str);
        generated.Seek(0, SeekOrigin.Begin);
        await Channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithFile("result.png", generated));

        await Channel.SendMessageAsync(BuildResult());

        if (!Channel.IsPrivate)
        {
            await Channel.SendMessageAsync(Locale.GameChannelInfo);
            await Task.Delay(60000);
            await Channel.DeleteAsync();
        }
        GameDatabaseHelper.RemoveGame(Id);
    }

    public override async Task OnInput(DiscordUser user, string input)
    {
        if (!_acceptInput) return;
        input = input.ToLower();

        if (input.Length != 5)
        {
            await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelRed,
                string.Format(Locale.GameInvalidLength, 5)));
            return;
        }

        if (!DictionaryDatabaseHelper.Exists(Language, input))
        {
            await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelRed,
                Locale.GameInvalidWord));
            return;
        }

        if (((List<string>) Info["GUESSES_RAW"]).Contains(input))
        {
            await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelRed,
                Locale.GameAlreadyUsed));
            return;
        }

        if (_lastResponseMessage != null) await _lastResponseMessage.DeleteAsync();
        var compared = WordleStringComparer.Compare(input, (string) Info["ANSWER"]);
        ((List<string>)Info["GUESSES"]).Add(compared.Item1);
        ((List<string>)Info["GUESSES_LETTERS"]).Add(compared.Item2);
        ((List<string>)Info["GUESSES_RAW"]).Add(input);
        _guess++;
        
        if (input == (string) Info["ANSWER"])
        {
            await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, Locale.GameWin));
            await OnEnd();
            return;
        }
        
        if (_guess == 6)
        {
            await Channel.SendMessageAsync(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow,
                string.Format(Locale.GameDefeat, (string)Info["ANSWER"])));
            await OnEnd();
            return;
        }
        
        var str = ((List<string>) Info["GUESSES"]).Aggregate("", (current, guess) => current + guess + "\n");
        str = str[..str.LastIndexOf("\n", StringComparison.Ordinal)];
        var stream = WordleDrawer.Generate(str);
        stream.Seek(0, SeekOrigin.Begin);
        _lastResponseMessage = await Channel.SendMessageAsync(new DiscordMessageBuilder()
                .WithFile($"wordle_{Id}_{_guess}.png", stream));
        Info["RESPONSE_MESSAGE"] = _lastResponseMessage.Id;
        Update();
    }

    private static Dictionary<char, string> _charToEmojiConverter = new Dictionary<char, string>
    {
        {'w', "⬛"},
        {'d', "🟨"},
        {'c', "🟩"}
    };
    public override string BuildResult()
    {
        var guesses = (List<string>) Info["GUESSES_LETTERS"];
        char guessCount;
        
        if (_guess < 6)
            guessCount = Convert.ToChar(_guess);
        else
            guessCount = guesses.Last().All(x => x == 'c') ? '6' : 'X';
        
        var str = $"Nedordle (Usual) {guessCount}/6\n";
        return guesses.Aggregate(str, (current1, guess) => guess.Aggregate(current1, (current, c) => current + _charToEmojiConverter[c]) + '\n');
    }
}
