using System.Drawing;
using DSharpPlus.Entities;

namespace Nedordle.Helpers;

public class SimpleDiscordEmbed
{
    private static DiscordEmbedBuilder Colored(DiscordColor color, string title = "", string description = "",
        string footer = "") => new DiscordEmbedBuilder()
        .WithTitle(title)
        .WithDescription(description)
        .WithFooter(footer)
        .WithColor(color);

    public static DiscordEmbedBuilder Error(string description, string footer = "", string title = "Error!") =>
        Colored(DiscordColor.DarkRed, title, description, footer);
    public static DiscordEmbedBuilder Warning(string description, string footer = "", string title = "Warning!") =>
        Colored(DiscordColor.DarkRed, title, description, footer);
    public static DiscordEmbedBuilder Success(string description, string footer = "", string title = "Success!") =>
        Colored(DiscordColor.DarkRed, title, description, footer);
}