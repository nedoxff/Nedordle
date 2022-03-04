using DSharpPlus.Entities;

namespace Nedordle.Game;

public abstract class MultiplayerGameHandler: GameHandler
{
    public Dictionary<ulong, DiscordChannel> Channels = new();
    public Dictionary<ulong, DiscordMessage?> ResponseMessages = new();
}