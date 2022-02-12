using System.Globalization;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Nedordle.Core.Types;
using Newtonsoft.Json;
using Serilog;

namespace Nedordle.Core.EventHandlers;

public class SlashCommandErrored
{
    public static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        if (!File.Exists("errors.json"))
        {
            Log.Information("errors.json does not exist, creating..");
            await File.WriteAllTextAsync("errors.json", "[]");
        }

        var guid = Guid.NewGuid().ToString();
        
        var deserialized = JsonConvert.DeserializeObject<List<Error>>(await File.ReadAllTextAsync("errors.json"));
        var error = new Error
        {
            Id = guid,
            Message = e.Exception.Message,
            Timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture),
            FullException = e.Exception.ToString(),
            AdditionalInformation = new Dictionary<string, object>
            {
                ["COMMAND_NAME"] = e.Context.CommandName,
                ["USER"] = e.Context.User.Id,
                ["GUILD"] = e.Context.Guild.Id
            }
        };
        deserialized!.Add(error);
        var serialized = JsonConvert.SerializeObject(deserialized, Formatting.Indented);
        await File.WriteAllTextAsync("errors.json", serialized);
        
        Log.Error($"An error occured while executing a slash command. Error ID: {guid}");

        await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder()
            .WithContent($"An error occured while executing your command.\nAn error report has been created, please wait for further messages.\nYour error ID: `{guid}`"));
    }
}