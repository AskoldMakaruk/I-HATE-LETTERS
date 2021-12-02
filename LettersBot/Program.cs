using BotFramework.Abstractions;
using BotFramework.Extensions.Hosting;
using BotFramework.Middleware;
using BotFramework.Services.Extensioins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

Host.CreateDefaultBuilder(args)
    .UseConfigurationWithEnvironment()
    .UseSerilog((context, configuration) =>
    {
        configuration
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    })
    .UseSimpleBotFramework((app, context) =>
    {
        app.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(context.Configuration["BotToken"]));

        app.Services.AddDbContext<LettersContext>(options =>
        {
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging();
        });

        app.Services.AddScoped<IUserRepository<User>, UserRepository>();
        app.UseIdentity<User>();
    })
    .Build()
    .Run();

public class User
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long ChatId { get; set; }
    public ReplaceType ReplaceType { get; set; } = ReplaceType.InvalidChars;
}

public class TextCommand : IStaticCommand
{
    private readonly User _user;

    public TextCommand(User user)
    {
        _user = user;
    }

    public bool SuitableFirst(Update update) => update?.Message?.Text != null
                                                && update?.Message?.Chat?.Type == ChatType.Private;

    public async Task Execute(IClient client)
    {
        var update = await client.GetUpdate();
        await client.SendTextMessage(LettersService.RemoveLetters(update.Message.Text, _user.ReplaceType));
    }
} 

public class ChannelCommand : IStaticCommand
{
    public bool SuitableFirst(Update update) => update?.ChannelPost?.Text != null 
                                                && update?.ChannelPost?.Chat?.Type == ChatType.Channel;

    public async Task Execute(IClient client)
    {
        var update = await client.GetUpdate();
        
        await client.EditMessageText(update.ChannelPost.MessageId, LettersService.RemoveLetters(update.ChannelPost.Text, ReplaceType.InvalidChars));
    }
};

public enum ReplaceType
{
    InvalidChars,
    Empty,
    Remove,
    SpecialChars
}

public static class LettersService
{
    public static Random Random = new();

    public static string RemoveLetters(string input, ReplaceType type)
    {
        return type switch
        {
            ReplaceType.Empty =>
                new string(input.Select(a => Constants.RemovedLetters.Contains(a) ? ' ' : a).ToArray()),
            ReplaceType.InvalidChars =>
                new string(input.Select(a => Constants.RemovedLetters.Contains(a) ? Constants.InvalidChar : a).ToArray()),
            ReplaceType.Remove =>
                new string(input.Select(a => Constants.RemovedLetters.Contains(a) ? '\0' : a).ToArray()),
            ReplaceType.SpecialChars =>
                new string(input.Select(a => Constants.RemovedLetters.Contains(a) ? Constants.Special[Random.Next(Constants.Special.Length)] : a).ToArray()),
            _ => input
        };
    }
}

public class Constants
{
    public const char InvalidChar = '�';
    public const string Alphabet = "йцукенгшщзхїфівапролджєячсмитьбю";
    public const string Special = "!@#$^&*?|~";
    public const string RemovedLetters = "ад";
}