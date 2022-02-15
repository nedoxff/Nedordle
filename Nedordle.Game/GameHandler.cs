using DSharpPlus.Entities;

namespace Nedordle.Game;

public abstract class GameHandler
{
    public string Id { get; init; }
    public List<DiscordUser> Players { get; } = new();
    public DiscordChannel Channel { get; init; }

    public abstract Task OnCreate();
    public abstract Task OnJoined(DiscordUser user);
    public abstract Task OnLeft(DiscordUser user);
    public abstract Task OnStart();
    public abstract Task OnEnd();
}