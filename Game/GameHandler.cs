using DSharpPlus.Entities;
using Nedordle.Database;

namespace Nedordle.Game;

public abstract class GameHandler
{
    public string GameType;
    public Dictionary<string, object> SpecificData = new();
    public string Id { get; init; }
    public List<DiscordUser> Players { get; } = new();
    public DiscordChannel Channel { get; init; }
    public string Language { get; init; }
    public string Locale { get; init; }

    public abstract Task OnCreate();
    public abstract Task OnJoined(DiscordUser user);
    public abstract Task OnLeft(DiscordUser user);
    public abstract Task OnStart();
    public abstract Task OnEnd();

    public virtual void Update()
    {
        GameDatabaseHelper.UpdateGame(this);
    }
}