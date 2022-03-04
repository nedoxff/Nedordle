using DSharpPlus.Entities;
using Nedordle.Database;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;
using Newtonsoft.Json;

namespace Nedordle.Game;

public class Player
{
    public readonly List<Guess> Guesses = new();
    public string GuessString = "";
    [JsonIgnore] public Locale Locale;
    [JsonIgnore] public DiscordUser User;

    public ulong UserId;

    public Player(DiscordUser user, Locale locale)
    {
        User = user;
        UserId = user.Id;
        Theme = Theme.Themes[UserDatabaseHelper.GetTheme(UserId)];
        LocaleString = locale.ShortName;
        Locale = locale;
    }

    public Theme Theme { get; }
    public string LocaleString { get; }

    public void AddGuess(string input, string answer)
    {
        var (full, clean) = WordleStringComparer.Compare(input, answer);
        GuessString += full + "\n";
        Guesses.Add(new Guess
        {
            Input = input,
            CleanOutput = clean,
            FormattedOutput = full
        });
    }
}