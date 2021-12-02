using BotFramework.Abstractions;
using BotFramework.Extensions;
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
using Telegram.Bot.Types.InlineQueryResults;

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

        app.Services.AddDbContext<LettersContext>(options => { options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")); });

        app.Services.AddScoped<IUserRepository<User>, UserRepository>();
        app.UseIdentity<User>();
    })
    .Build()
    .Run();

public class InlineQueryCommand : IStaticCommand
{
    public bool SuitableFirst(Update update) => update?.InlineQuery?.Query != null;

    public async Task Execute(IClient client)
    {
        var update = await client.GetUpdate();
        var results =
            new[]
                {
                    ReplaceType.InvalidChars,
                    ReplaceType.Remove,
                    ReplaceType.SpecialChars
                }.Select(a => LettersService.RemoveLetters(update.InlineQuery.Query, a))
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => new InlineQueryResultArticle(Guid.NewGuid().ToString(), a, new InputTextMessageContent(a)))
                .ToArray();

        await client.AnswerInlineQuery(update.InlineQuery.Id, results);
    }
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

public class BanCommand : IStaticCommand
{
    public bool SuitableFirst(Update update) => update?.Message?.Text != null && update?.Message?.Chat?.Type == ChatType.Group || update?.Message?.Chat?.Type == ChatType.Supergroup;

    public async Task Execute(IClient client)
    {
        var update = await client.GetUpdate();
        if (update.Message.Text.Any(a => Constants.RemovedLetters.Contains(a)))
        {
            await client.DeleteMessage(update.Message.MessageId);
        }
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