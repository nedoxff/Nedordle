using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Helpers;

namespace Nedordle.Commands.General;

public class Troubleshoot: ExtendedCommandModule
{
    [SlashCommand("troubleshoot", "The bot doesn't work as intended? Try running this command.")]
    public async Task Execute(InteractionContext ctx)
    {
        var task = new MessageTaskListBuilder(ctx)
            .WithTitle("Finding issues..")
            .WithColor(DiscordColor.Gold)
            .AddTask("Checking the database")
            .Build();

        var guildExists = GuildDatabaseHelper.GuildExists(ctx.Guild.Id);
        task.Log(guildExists ? "The server is in the database.": "The server is not in the database!");
        await task.FinishTask();
        
        //TODO: other troubleshooting things

        var followUp = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(DiscordColor.Gold, "", "Fixing problems..")));
        
        if(!guildExists)
            GuildDatabaseHelper.AddGuild(ctx.Guild.Id);

        await ctx.EditFollowupAsync(followUp.Id, new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(DiscordColor.DarkGreen, "", "Done!")));
    }
}