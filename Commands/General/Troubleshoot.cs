using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Helpers;

namespace Nedordle.Commands.General;

public class Troubleshoot : ExtendedCommandModule
{
    [SlashCommand("troubleshoot", "The bot doesn't work as intended? Try running this command.")]
    public async Task Execute(InteractionContext ctx)
    {
        var task = new MessageTaskListBuilder(ctx)
            .WithTitle(Locale.TroubleshootFindingIssues)
            .WithColor(SimpleDiscordEmbed.PastelYellow)
            .AddTask(Locale.TroubleshootServer)
            .IsPrivate()
            .Build();

        var problems = new List<ProblemType>();

        var guildExists = GuildDatabaseHelper.GuildExists(ctx.Guild.Id);
        if (!guildExists)
            problems.Add(ProblemType.GuildNotInDatabase);
        await task.FinishTask();

        //TODO: other troubleshooting things

        if (problems.Count != 0)
        {
            var followUp = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, "", Locale.TroubleshootFixing))
                .AsEphemeral(true));

            foreach (var problem in problems)
                FixProblem(ctx, problem);

            await ctx.EditFollowupAsync(followUp.Id, new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, "", Locale.TroubleshootDone)));
        }
        else
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, "", Locale.TroubleshootNoIssues))
                .AsEphemeral(true));
        }
    }

    private void FixProblem(BaseContext ctx, ProblemType type)
    {
        switch (type)
        {
            case ProblemType.GuildNotInDatabase:
                GuildDatabaseHelper.AddGuild(ctx.Guild.Id);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private enum ProblemType
    {
        GuildNotInDatabase
    }
}