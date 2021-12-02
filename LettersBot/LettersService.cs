public static class LettersService
{
    public static Random Random = new();

    public static string RemoveLetters(string input, ReplaceType type)
    {
        return type switch
        {
            ReplaceType.Empty =>
                new string(input.Select(a => Constants.RemovedLetters.Contains(a, StringComparison.InvariantCultureIgnoreCase) ? ' ' : a).ToArray()),
            ReplaceType.InvalidChars =>
                new string(input.Select(a => Constants.RemovedLetters.Contains(a, StringComparison.InvariantCultureIgnoreCase) ? Constants.InvalidChar : a).ToArray()),
            ReplaceType.Remove =>
                new string(input.Where(a => !Constants.RemovedLetters.Contains(a, StringComparison.InvariantCultureIgnoreCase)).ToArray()),
            ReplaceType.SpecialChars =>
                new string(input.Select(a => Constants.RemovedLetters.Contains(a, StringComparison.InvariantCultureIgnoreCase) ? Constants.Special[Random.Next(Constants.Special.Length)] : a).ToArray()),
            _ => input
        };
    }
}