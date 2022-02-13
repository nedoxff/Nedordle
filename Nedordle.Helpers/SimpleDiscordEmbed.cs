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

    public static DiscordEmbedBuilder Error(string description, string footer = "", string title = "Error!")
    {
        return Colored(DiscordColor.DarkRed, title, description, footer);
    }

    public static DiscordEmbedBuilder Warning(string description, string footer = "", string title = "Warning!")
    {
        return Colored(DiscordColor.DarkRed, title, description, footer);
    }

    public static DiscordEmbedBuilder Success(string description, string footer = "", string title = "Success!")
    {
        return Colored(DiscordColor.DarkRed, title, description, footer);
    }
}