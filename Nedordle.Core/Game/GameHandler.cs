using DSharpPlus.Entities;

namespace Nedordle.Core.Game;

public abstract class GameHandler
{
    public string Id { get; }
    public List<DiscordUser> Players { get; } = new();
    public DiscordChannel Channel { get; }

    public abstract Task OnCreate();
    public abstract Task OnJoined(DiscordUser user);
    public abstract Task OnLeft(DiscordUser user);
    public abstract Task OnStart();
    public abstract Task OnEnd();
}