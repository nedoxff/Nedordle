using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;

namespace Nedordle.Commands.General;

public class Troubleshoot : ExtendedCommandModule
{
    [SlashCommand("troubleshoot", "The bot doesn't work as intended? Try running this command.")]
    public async Task Execute(InteractionContext ctx)
    {
        var task = new MessageTaskListBuilder(ctx)
            .WithTitle("Finding issues..")
            .WithColor(SimpleDiscordEmbed.PastelYellow)
            .AddTask("Checking for server in the database")
            .AddTask("Checking if the locale is valid")
            .IsPrivate()
            .Build();

        var problems = new List<ProblemType>();

        var guildExists = GuildDatabaseHelper.GuildExists(ctx.Guild.Id);
        task.Log(guildExists ? "The server is in the database." : "The server is not in the database!");
        if (!guildExists)
            problems.Add(ProblemType.GuildNotInDatabase);
        await task.FinishTask();

        var locale = LocaleDatabaseHelper.GetLocale(ctx.Guild.Id);
        if (string.IsNullOrEmpty(locale))
        {
            task.Log("No locale detected, the server is not in the database.");
            problems.Add(ProblemType.InvalidLocale);
        }
        else if (!Locale.Locales.ContainsKey(locale))
        {
            task.Log("Invalid locale \"{0}\".", locale);
            problems.Add(ProblemType.InvalidLocale);
        }
        else
        {
            task.Log("Valid locale detected. (\"{0}\")", locale);
        }

        await task.FinishTask();

        //TODO: other troubleshooting things

        if (problems.Count != 0)
        {
            var followUp = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, "", "Fixing problems.."))
                .AsEphemeral(true));

            foreach (var problem in problems)
                FixProblem(ctx, problem);

            await ctx.EditFollowupAsync(followUp.Id, new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, "", "Done!")));
        }
        else
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, "", "No problems found."))
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
            case ProblemType.InvalidLocale:
                GuildDatabaseHelper.SetLocale(ctx.Guild.Id, "en");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private enum ProblemType
    {
        GuildNotInDatabase,
        InvalidLocale
    }
}