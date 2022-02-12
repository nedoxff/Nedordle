<h1 align="center">Nedordle<br>Wordle, but in Discord!</h1>

Nedordle is a [Discord](https://discord.com) bot that lets you play the famous game Wordle in your DMs,
or with your friends!

## Features

- It's a Discord bot! :D
- Support for 3-∞ character long words.
- Multiple game modes, like:
    - Usual
    - Speedrun
    - Team
    - And more!
- User-friendly CLI (almost TUI) for configuring the bot.
- TODO:tm:

## How do I run it?

Well to be honest I don't think you need another instance of Nedordle, but if you *really* want to host it yourself, you can by doing this:

- Install .NET 6.0 (TODO:tm: instructions)
- Clone the repository: `git clone https://github.com/NedoProgrammer/Nedordle`
- Compile the project: `cd Nedordle && dotnet build --configuration Release`
- When finished, go to the build directory: `cd Nedordle.Cli/bin/Release/net6.0`
- Run `./Nedordle.Cli interactive` and select `Download required data and configure the database`
- Wait for the process to finish

Now, you can start the bot using three methods of authefication:

- Create an .env file with the following properties:
    - TOKEN - token of your Discord bot
    - GUILD_ID (optional) - the guild for which the slash commands will update immediately (if not specified, they will be updated in one hour)

Then you can just call `./Nedordle.Cli`.

- Manually specify token and guild ID using `--token` and `--guild-id`: `./Nedordle.Cli --token YOUR_TOKEN --guild-id YOUR_GUILD_ID`
- Manually specify token and guild ID using the interactive menu:
    - Run `./Nedordle.Cli interactive`
    - Select `Run the bot using manual settings`
    - Enter the required data (don't worry, the token is hidden)

After that, you should get a lot of messages from the Discord API (DSharpPlus), that means everything works correctly. Congrats :D

## Credits

**Josh Wardle** for creating Wordle!

**The DSharpPlus team** for creating DSharpPlus!
