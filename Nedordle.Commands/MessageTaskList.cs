using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Nedordle.Commands;

public enum MessageTaskState
{
    Done,
    InProgress,
    Failed
}

public class MessageTask
{
    public string Name { get; }
    public string Log = "";
    public MessageTaskState State = MessageTaskState.InProgress;
    public MessageTask(string name) => Name = name;
}

public class MessageTaskFailedException: Exception
{
    public MessageTask Task { get; }
    public string CommandName { get; }
    public MessageTaskFailedException(string message, string commandName, MessageTask task) : base(message)
    {
        Task = task;
        CommandName = commandName;
    }
}

public class MessageTaskList
{
    private DiscordEmbedBuilder _builder;
    private Dictionary<string, MessageTask> _tasks;
    private InteractionContext _context;
    private int _index;
    private string _successDescription;
    private bool _finished;
    
    public void Log(string message) => _tasks.ElementAt(_index).Value.Log += message + "\n";
    public void Log(string template, params object[] objs) => _tasks.ElementAt(_index).Value.Log += string.Format(template, objs) + "\n";

    public MessageTaskList(DiscordEmbedBuilder builder, Dictionary<string, MessageTask> tasks, InteractionContext ctx, string successDescription = "")
    {
        _successDescription = successDescription;
        _builder = builder;
        _tasks = tasks;
        _context = ctx;
        Init().GetAwaiter().GetResult();
    }

    private async Task Init()
    {
        await _context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await UpdateContent();
    }

    public async Task FinishTask()
    {
        if (_finished) return;
        _tasks.ElementAt(_index).Value.State = MessageTaskState.Done;
        _index++;
        if (_index <= _tasks.Count - 1)
            await UpdateContent();
        else
        {
            _finished = true;
            await UpdateContent(true);
        }
    }

    public async Task FailTask(string reason)
    {
        _tasks.ElementAt(_index).Value.State = MessageTaskState.Failed;
        await UpdateContent();
        throw new MessageTaskFailedException(reason, _context.CommandName, _tasks.ElementAt(_index).Value);
    }
    
    private Dictionary<MessageTaskState, char> _stateConverter = new()
    {
        {MessageTaskState.Done, '✓'},
        {MessageTaskState.Failed, '✗'},
        {MessageTaskState.InProgress, '◌'}
    };
    public async Task UpdateContent(bool addDescription = false)
    {
        var str = $"{(addDescription && !string.IsNullOrEmpty(_successDescription) ? $"**{_successDescription}**\n": "")}```\n";
        for (var i = 0; i < _tasks.Count; i++)
        {
            var task = _tasks.ElementAt(i).Value;
            str += $"[{_stateConverter[task.State]}] {task.Name}";
            if (!string.IsNullOrEmpty(task.Log))
                str += $"\n{task.Log}";
            else
                str += '\n';
        }
        str += "```";
        
        await _context.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(_builder.WithDescription(str).Build()));
    }
}

public class MessageTaskListBuilder
{
    private readonly DiscordEmbedBuilder _builder = new();
    private Dictionary<string, MessageTask> _tasks = new();
    private InteractionContext _context;
    private string _successDescription = "";

    public MessageTaskListBuilder(InteractionContext context) => _context = context;

    public MessageTaskListBuilder WithSuccessDescription(string description)
    {
        _successDescription = description;
        return this;
    }
    
    public MessageTaskListBuilder WithTitle(string title)
    {
        _builder.Title = title;
        return this;
    }

    public MessageTaskListBuilder WithColor(DiscordColor color)
    {
        _builder.Color = color;
        return this;
    }

    public MessageTaskListBuilder AddTask(string title)
    {
        _tasks[title] = new MessageTask(title);
        return this;
    }

    public MessageTaskList Build() => new(_builder, _tasks, _context, _successDescription);
}