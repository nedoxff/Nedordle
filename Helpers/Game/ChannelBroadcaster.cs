using DSharpPlus.Entities;
using Nedordle.Game;
using Nedordle.Helpers.Types;

namespace Nedordle.Helpers.Game;

public class ChannelBroadcaster
{
    public enum BroadcastLevel
    {
        Warning,
        Error,
        Success
    }

    public static async Task Broadcast(MultiplayerGameHandler handler, BroadcastLevel level, string propertyName, params object[] objs)
    {
        DiscordEmbedBuilder builder;
        switch (level)
        {
            case BroadcastLevel.Warning:
                builder = SimpleDiscordEmbed.Warning();
                break;
            case BroadcastLevel.Error:
                builder = SimpleDiscordEmbed.Error();
                break;
            case BroadcastLevel.Success:
                builder = SimpleDiscordEmbed.Success();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
        
        foreach (var (key, value) in handler.Channels)
            await value.SendMessageAsync(builder.WithDescription(GetLocaleProperty(handler, key, propertyName, objs)));
    }

    private static string GetLocaleProperty(GameHandler handler, ulong id, string propertyName, params object[] objs)
    {
        var locale = handler.Players[id].Locale;
        var field = typeof(Locale).GetField(propertyName);
        if (field == null) return propertyName;
        var str =
            (string?) field.GetValue(locale) ?? throw new InvalidOperationException();
        return objs.Length != 0 ? string.Format(str, objs) : str;
    }
}