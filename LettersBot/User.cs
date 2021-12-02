public class User
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long ChatId { get; set; }
    public ReplaceType ReplaceType { get; set; } = ReplaceType.InvalidChars;
}