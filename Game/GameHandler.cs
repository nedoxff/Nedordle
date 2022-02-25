using DSharpPlus.Entities;
using Nedordle.Database;
using Nedordle.Helpers.Types;

namespace Nedordle.Game;

public abstract class GameHandler
{
    public static Dictionary<string, GameHandler> Handlers = new();

    public readonly Dictionary<string, object> Info = new();

    public string GameType { get; init; }
    public string Id { get; init; }
    public Dictionary<ulong, Player> Players { get; } = new();
    public DiscordChannel Channel { get; init; }
    public string Language { get; init; }
    public bool Ended { get; protected set; } = false;
    public bool Playing { get; protected set; } = false;

    public abstract Task OnCreate(DiscordUser creator, Locale locale);
    public abstract Task OnJoined(DiscordChannel callerChannel, DiscordUser user, Locale locale);
    public abstract Task OnLeft(DiscordChannel callerChannel, DiscordUser user, Locale locale);
    public abstract Task OnStart();
    public abstract Task OnEnd();
    public abstract Task OnCleanup();
    public abstract Task OnInput(DiscordUser user, string input);
    public abstract string BuildResult(Player player);

    public void Update()
    {
        GameDatabaseHelper.UpdateGame(this);
    }
}