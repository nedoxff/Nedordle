using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Nedordle.Game;

public class Player
{
    [JsonIgnore]
    public DiscordUser User;
    public ulong UserId;
    public List<Guess> Guesses = new();
}