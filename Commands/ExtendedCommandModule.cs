using DSharpPlus.SlashCommands;
using Nedordle.Helpers.Types;
using Serilog;

namespace Nedordle.Commands;

public class ExtendedCommandModule : ApplicationCommandModule
{
    public Locale Locale { get; private set; } = null!;

    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        Log.Debug("Trying to get locale for user {UserId}..", ctx.User.Id);
        var locale = ctx.Interaction.Locale;
        if (!Locale.Locales.Any(x => locale.StartsWith(x.Key)))
        {
            Log.Debug("Invalid locale \"{Locale}\", using \"en\"", locale);
            Locale = Locale.Locales["en"];
        }
        else
        {
            var nedordleLocale = Locale.Locales.First(x => locale.StartsWith(x.Key)).Key;
            Log.Debug("The language for this user is \"{Locale}\"", nedordleLocale);
            Locale = Locale.Locales[nedordleLocale];
        }

        return Task.FromResult(true);
    }
}