using DSharpPlus.Entities;

namespace Nedordle.Helpers;

public class SimpleDiscordEmbed
{
    public static DiscordEmbedBuilder Colored(DiscordColor color, string title = "", string description = "",
        string footer = "")
    {
        return new DiscordEmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithFooter(footer)
            .WithColor(color);
    }

    public static DiscordEmbedBuilder Error(string description, string footer = "", string title = "")
    {
        return Colored(DiscordColor.DarkRed, title, description, footer);
    }

    public static DiscordEmbedBuilder Warning(string description, string footer = "", string title = "")
    {
        return Colored(DiscordColor.Gold, title, description, footer);
    }

    public static DiscordEmbedBuilder Success(string description, string footer = "", string title = "")
    {
        return Colored(DiscordColor.DarkGreen, title, description, footer);
    }
}