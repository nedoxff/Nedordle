using Nedordle.Game;
using Nedordle.Game.Handlers;

namespace Nedordle.Helpers.Game;

public class EmojiResultBuilder
{
    public static string BuildDefault(Player player, string gameMode, int attemptLimit)
    {
        var guessCount = player.Guesses.Count;
        string guessString;
        if (attemptLimit == -1 || guessCount < attemptLimit) guessString = guessCount.ToString();
        else guessString = player.Guesses.Last().CleanOutput.All(x => x == 'c') ? attemptLimit.ToString() : "X";

        var theme = player.Theme;
        var str = $"Nedordle ({gameMode}) {guessString}/{(attemptLimit == -1 ? '∞' : attemptLimit.ToString())}\n\n";
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

    public static string BuildTeamRace(Player player, ulong winner, string gameMode)
    {
        var guessCount = player.Guesses.Count;

        var guessString = winner == player.UserId ? guessCount.ToString() : "X";
        var theme = player.Theme;
        var str = $"Nedordle ({gameMode}) {guessString}/∞\n\n";
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

    public static string BuildTeamWork(TeamworkGameHandler handler, Player player)
    {
        var guesses = handler.Guesses;
        var guessCount = guesses.Count;

        string guessString;
        if (handler.AttemptLimit == -1 || guessCount < handler.AttemptLimit) guessString = guessCount.ToString();
        else guessString = guesses.Last().Item1.CleanOutput.All(x => x == 'c') ? handler.AttemptLimit.ToString() : "X";
        var theme = player.Theme;
        var str = $"Nedordle ({player.Locale.GameTypes["teamwork"].Name}) {guessString}/{(handler.AttemptLimit == -1 ? '∞' : handler.AttemptLimit.ToString())}\n\n";
        foreach (var guess in guesses)
        {
            foreach (var c in guess.Item1.CleanOutput)
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
            
            str += $" ({guess.Item2})\n";
        }

        return str;
    }
}