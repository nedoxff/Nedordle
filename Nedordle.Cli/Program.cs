using System.Diagnostics.CodeAnalysis;
using Nedordle.Cli.DatabaseConfiguration;
using Nedordle.Core;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<RunCommand>();
app.Configure(s =>
{
    s.AddCommand<InteractiveCommand>("interactive")
        .WithExample(new[] {"interactive"})
        .WithDescription("Enter the interactive menu");
    s.SetExceptionHandler(e =>
    {
        AnsiConsole.MarkupLine($"[bold red]An exception occured while running the app: {e.Message}[/]");
        var show = AnsiConsole.Confirm("Would you like to see the full exception?");
        if(show)
            AnsiConsole.WriteException(e);
    });
});
app.Run(args);

public class RunCommand : Command<RunCommand.RunCommandOptions>
{
    // ReSharper disable twice RedundantNullableFlowAttribute
    public override int Execute([NotNull] CommandContext context, [NotNull] RunCommandOptions settings)
    {
        var config = new Config();
        if (string.IsNullOrEmpty(settings.Token) && settings.GuildId == 0)
        {
            config.LoadFromEnvironment();
        }
        else
        {
            if (settings.GuildId != 0 && string.IsNullOrEmpty(settings.Token))
                throw new Exception("The token cannot be null or empty!");
            config["TOKEN"] = settings.Token;
            config["GUILD_ID"] = settings.GuildId.ToString();
        }

        AnsiConsole.Clear();
        Client.Start(config).ConfigureAwait(false).GetAwaiter().GetResult();
        return 0;
    }

    public class RunCommandOptions : CommandSettings
    {
        [CommandOption("--token")] public string Token { get; set; } = "";

        [CommandOption("--guild-id")] public ulong GuildId { get; set; }
    }
}

public class InteractiveCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.Clear();
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .PageSize(10)
            .Title("I would like to..")
            .MoreChoicesText("Scroll down (press the down arrow) to see more actions.")
            .AddChoices("Run the bot using the .env file", "Run the bot using manual settings",
                "Download required data and configure the database", "Exit the app"));

        switch (choice)
        {
            case "Run the bot using the .env file":
            {
                AnsiConsole.Clear();
                var config = new Config();
                config.LoadFromEnvironment();
                Client.Start(config).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            }
            case "Run the bot using manual settings":
            {
                AnsiConsole.Clear();

                var token = AnsiConsole.Prompt(new TextPrompt<string>("What is the token of your bot?")
                    .Secret());
                var update = AnsiConsole.Confirm("Would you like to update commands on a certain guild?");
                ulong guildId = 0;
                if (update)
                    guildId = AnsiConsole.Ask<ulong>("What is the ID of that guild?");

                AnsiConsole.Clear();
                var config = new Config
                {
                    ["TOKEN"] = token,
                    ["GUILD_ID"] = update ? guildId.ToString() : ""
                };
                Client.Start(config).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            }
            case "Download required data and configure the database":
                AnsiConsole.Clear();
                InteractiveConfigure.Configure();
                break;
            case "Exit the app":
                Environment.Exit(0);
                break;
        }

        return 0;
    }
}