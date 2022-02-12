using DSharpPlus.SlashCommands;
using Nedordle.Core.Types;
using Nedordle.Database;
using Serilog;

namespace Nedordle.Commands;

public class ExtendedCommandModule: ApplicationCommandModule
{
    public Locale Locale { get; private set; }
    public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        Log.Debug($"Trying to get locale for guild {ctx.Guild.Id}..");
        var locale = LocaleDatabaseHelper.GetLocale(ctx.Guild.Id);
        if (string.IsNullOrEmpty(locale))
        {
            Log.Debug("Guild was not in the database. Using \"en\".");
            Locale = Locale.Locales["en"];
        }
        else
        {
            Log.Debug($"The language for this guild is \"{locale}\".");
            Locale = Locale.Locales[locale];
        }
        return true;
    }
}