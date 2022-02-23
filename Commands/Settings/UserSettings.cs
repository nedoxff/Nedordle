using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Helpers;

namespace Nedordle.Commands.Settings;

public partial class Settings: ExtendedCommandModule
{
    private string _theme;
    
    [SlashCommand("user", "Change personal settings.")]
    public async Task ExecuteUser(InteractionContext ctx)
    {
        InitUser(ctx.User.Id);
        
        var first = true;
        DiscordMessage message;
        
        while (true)
        {
            var (embed, buttons) = GetUserData(ctx.User);
            if (first)
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AddComponents(buttons)
                    .AsEphemeral());
                message = await ctx.GetOriginalResponseAsync();
                first = false;
            }
            else
            {
                message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(embed)
                    .AddComponents(buttons));   
            }

            var result = await message.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(1));
            if (result.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
                break;
            }
            
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            var action = result.Result.Id;

            if (await HandleUserAction(ctx, message, action))
                break;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, Locale.ApplyingChanges)));
        
        //APPLY

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, Locale.Done)));
    }

    private void InitUser(ulong id)
    {
        _theme = UserDatabaseHelper.GetTheme(id);
    }

    private async Task<bool> HandleUserAction(InteractionContext ctx, DiscordMessage message, string action)
    {
        switch (action)
        {
            case "selectTheme":
            {
                return false;
            }
            case "apply":
                return true;
            default:
                return false;
        }
    }

    private (DiscordEmbed, List<DiscordButtonComponent>) GetUserData(DiscordUser user)
    {
        var buttons = new List<DiscordButtonComponent>
        {
            new(ButtonStyle.Secondary, "selectTheme", "LOCALE_SELECT_THEME"),
            new(ButtonStyle.Success, "apply", Locale.Apply)
        };

        var description = $@"
{{LOCALE_THEME}}: `{_theme}`
";
        
        var embed = new DiscordEmbedBuilder()
            .WithTitle("LOCALE_USER_SETTINGS")
            .WithColor(SimpleDiscordEmbed.PastelYellow)
            .WithDescription(description)
            .WithTimestamp(DateTime.Now)
            .Build();

        return (embed, buttons);
    }
}