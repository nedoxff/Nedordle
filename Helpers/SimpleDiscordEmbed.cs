using DSharpPlus.Entities;

namespace Nedordle.Helpers;

public class SimpleDiscordEmbed
{
    public static DiscordColor PastelRed = new(255, 105, 97);
    public static DiscordColor PastelYellow = new(253, 253, 150);
    public static DiscordColor PastelGreen = new(119, 221, 119);

    public static DiscordEmbedBuilder Colored(DiscordColor color, string description = "", string title = "",
        string footer = "")
    {
        return new DiscordEmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithFooter(footer)
            .WithColor(color);
    }

    public static DiscordEmbedBuilder Error(string description = "", string footer = "", string title = "")
    {
        return Colored(PastelRed, description, title, footer);
    }

    public static DiscordEmbedBuilder Warning(string description = "", string footer = "", string title = "")
    {
        return Colored(PastelYellow, description, title, footer);
    }

    public static DiscordEmbedBuilder Success(string description = "", string footer = "", string title = "")
    {
        return Colored(PastelGreen, description, title, footer);
    }
}