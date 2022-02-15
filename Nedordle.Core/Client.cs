using System.Reflection;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Nedordle.Commands.General;
using Nedordle.Core.EventHandlers;
using Nedordle.Database;
using Nedordle.Drawer;
using Nedordle.UptimeServer;
using Serilog;
using Serilog.Events;

namespace Nedordle.Core;

public class Client
{
    private const string DefaultServerResponseString = "I don't know what you expect to see here.";
    private static DiscordClient _client = null!;
    private static readonly string[] Files = {"latest_log.txt", "latest_log_debug.txt"};

    //TODO: add command handlers
    public static async Task Start(Config config)
    {
        InitializeLogger();
        CheckResources();

        var stream = NewGameDrawer.Generate("Mathmems");
        var file = new FileStream("test.png", FileMode.OpenOrCreate, FileAccess.Write);
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(file);
        stream.Close();
        file.Close();


        UptimeListener.Start(config.Contains("SERVER_RESPONSE_STRING")
            ? config["SERVER_RESPONSE_STRING"]
            : DefaultServerResponseString);

        var logFactory = new LoggerFactory().AddSerilog();
        _client = new DiscordClient(new DiscordConfiguration
        {
            Token = config["TOKEN"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.Guilds,
            LoggerFactory = logFactory,
            MinimumLogLevel = LogLevel.Debug
        });
        _client.GuildCreated += GuildCreated.OnGuildCreated;
        _client.GuildDeleted += GuildDeleted.OnGuildDeleted;

        var slash = _client.UseSlashCommands();
        slash.SlashCommandErrored += SlashCommandErrored.OnSlashCommandErrored;
        slash.RegisterCommands(Assembly.GetAssembly(typeof(Ping)), ulong.Parse(config["GUILD_ID"]));
        Log.Information("Initialized slash commands");

        _client.UseInteractivity();
        Log.Information("Initialized interactivity");

        Log.Information("Loading locales..");
        LocaleDatabaseHelper.LoadLocales();

        await _client.ConnectAsync();
        await Task.Delay(-1);
    }

    private static void InitializeLogger()
    {
        foreach (var file in Files)
            if (File.Exists(file))
                File.Delete(file);
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .WriteTo.Console()
#else
            .WriteTo.Console(LogEventLevel.Information)
#endif
            .WriteTo.File("latest_log.txt", LogEventLevel.Information)
            .WriteTo.File("latest_log_debug.txt", LogEventLevel.Debug)
            .MinimumLevel.Debug()
            .CreateLogger();
        Log.Information("Logger initialized");
    }

    private static void CheckResources()
    {
        Log.Information("Checking resources..");
        ThrowIfDoesNotExist("database.db");
        ThrowIfDoesNotExist("Resources");
        ThrowIfDoesNotExist("Resources/wordle_font.ttf");
        Log.Information("All fine!");
    }

    private static void ThrowIfDoesNotExist(string file)
    {
        if (!Directory.Exists(file) && !File.Exists(file))
            throw new FileNotFoundException($"Requested resource \"{file}\" was not found.");
    }
}