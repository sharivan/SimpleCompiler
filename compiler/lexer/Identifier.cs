namespace compiler.lexer;

public class Identifier : Token
{
    public static bool IsLetter(char c)
    {
        return c is >= 'A' and <= 'Z' or
            >= 'a' and <= 'z' or
            not '\u00d7' and >= '\u00c0' and <= '\u00dd' or
            not '\u00f7' and >= '\u00e0' and <= '\u00ff';
    }

    public static bool CanBeAVariableIntentifier(string name)
    {
        if (name.Length == 0)
            return false;

        if (!IsLetter(name[0]))
            return false;

        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (!IsLetter(c) && c != '_')
                return false;
        }

        return true;
    }

    public string Name
    {
        get;
    }

    internal Identifier(SourceInterval interval, string name) : base(interval)
    {
        Name = name;
    }

    public override string ToString()
    {
        return $"identifier '{Name}'";
    }
}
