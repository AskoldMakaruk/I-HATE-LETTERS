var dir = Directory.GetCurrentDirectory() + "\\xmls";
var resultDir = Directory.GetCurrentDirectory() + "\\results";

Directory.CreateDirectory(resultDir);

foreach (var file in Directory.EnumerateFiles(dir))
{
    var lines = File.ReadAllLines(file);
    for (var i = 0; i < lines.Length; i++)
    {
        if (file.EndsWith(".strings"))
        {
            lines[i] = ClearStrings(lines[i]);
        }
        else if (file.EndsWith(".xml"))
        {
            lines[i] = ClearXml(lines[i]);
        }
    }


    var path = Path.Combine(resultDir, Path.GetFileName(file));
    File.WriteAllLines(path, lines);
}

static string ClearXml(string line)
{
    if (!line.Contains("string"))
    {
        return line;
    }

    var parts = new[]
    {
        line[..line.IndexOf('>')],
        line[line.IndexOf('>')..line.LastIndexOf('<')],
        line[line.LastIndexOf('<')..],
    };

    return parts[0] + RemoveLetters(parts[1]) + parts[2];
}


static string ClearStrings(string line)
{
    if (!line.Contains('='))
    {
        return line;
    }

    var parts = line.Split('=');

    return parts[0] + RemoveLetters(parts[1]);
}

static string RemoveLetters(string s)
{
    var bannedLetters = "аАaA";
    return new string(s.Where(a => !bannedLetters.Contains(a)).ToArray());
}