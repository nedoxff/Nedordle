using DSharpPlus.Entities;
using Nedordle.Database;
using Nedordle.Helpers.Types;

namespace Nedordle.Game;

public abstract class GameHandler
{
    public static Dictionary<string, GameHandler> Handlers = new();

    public string GameType;
    public Dictionary<string, object> Info = new();
    public string Id { get; init; }
    public List<Player> Players { get; } = new();
    public DiscordChannel Channel { get; init; }
    public string Language { get; init; }
    public string LocaleString { get; init; }
    public Locale Locale => Locale.Locales[LocaleString];

    public abstract Task OnCreate();
    public abstract Task OnJoined(DiscordUser user);
    public abstract Task OnLeft(DiscordUser user);
    public abstract Task OnStart();
    public abstract Task OnEnd();
    public abstract Task OnInput(DiscordUser user, string input);
    public abstract string BuildResult();

    public void Update() => GameDatabaseHelper.UpdateGame(this);
}