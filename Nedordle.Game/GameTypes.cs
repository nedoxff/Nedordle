using DSharpPlus.Entities;

namespace Nedordle.Game;

public struct GameType
{
    public string Name;
    public string FullDescription;
    public string ShortDescription;
    public DiscordEmoji Emoji;
    public bool IsMultiplayer;
}

public class GameTypes
{
    public static Dictionary<string, GameType> Types = new()
    {
        ["usual"] = new()
        {
            Name = "Usual",
            ShortDescription = "Standard, one player wordle with 5 letters.",
            FullDescription =
                "The same wordle you would find in https://www.nytimes.com/games/wordle/index.html, but in Discord.",
            Emoji = DiscordEmoji.FromUnicode("🟩"),
            IsMultiplayer = false
        }
    };
}