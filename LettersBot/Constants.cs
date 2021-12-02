public static class Constants
{
    public const char InvalidChar = '�';

    public static DateOnly Start = new DateOnly(2021, 12, 1);

    public static char[] Alphabet = new[]
    {
        'а',
        'т',
        'г',
        'є',
        'л',
        'о',
        'і',
        'ц',
        'п',
        'р',
        'х',
        'ї',
        'д',
        'с',
        'у',
        'ш',
        'ф',
        'я',
        'е',
        'з',
        'ь',
        'й',
        'щ',
        'н',
        'б',
        'и',
        'к',
        'ч',
        'в',
        'ж',
        'ю',
        'м',
    };

    public const string Special = "!@#$^&*?|~";
    private static int DaysPast = (DateTime.Now - Start.ToDateTime(TimeOnly.MinValue)).Days;
    public static string RemovedLetters => new string(Alphabet[..DaysPast]);
}