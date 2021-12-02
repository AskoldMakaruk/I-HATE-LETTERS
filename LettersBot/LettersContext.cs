using BotFramework.Middleware;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

public class UserRepository : IUserRepository<User>
{
    private readonly LettersContext _context;

    public UserRepository(LettersContext context)
    {
        _context = context;
    }

    public User FromId(int id) => _context.Users.Find(id);

    public async Task<User> FromMessage(Message message)
    {
        var account = _context.Users.FirstOrDefault(a => a.ChatId == message.Chat.Id);

        if (account == null)
        {
            account = await CreateUser(message.From);
        }

        return account;
    }


    internal void RemoveAccount(User user)
    {
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    public async Task<User?> GetUser(long userId)
    {
        return await _context.Users.FirstOrDefaultAsync(a => a.ChatId == userId);
    }

    public async Task<User> CreateUser(global::Telegram.Bot.Types.User user)
    {
        var account = new User
        {
            ChatId = user.Id,
            Name = user.Username
        };
        if (user.Username == null)
        {
            account.Name = user.FirstName + " " + user.LastName;
        }

        await _context.Users.AddAsync(account);
        await _context.SaveChangesAsync();
        return account;
    }
}

public class LettersContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public LettersContext(DbContextOptions options) : base(options) 
    {
        
    }
}