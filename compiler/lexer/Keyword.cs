namespace compiler.lexer;

public class Keyword : Token
{
    public static readonly string[] KEYWORDS = { 
        // tipos
        "void",
        "bool",
        "byte",
        "char",
        "short",
        "int",
        "long",
        "float",
        "real", // double
        "texto",
        // conversão de tipo
        "cast",
        // declarações
        "var",
        "função",
        "externa",
        "estrutura",
        "programa",
        "unidade",
        "usando",
        // leitura e escrita
        "leia",
        "escreva",
        "escrevaln",
        // literais lógicos
        "verdade", // true
        "falso", // false
        // literal nulo
        "nulo", // null
        // estruturas de controle de fluxo
        "se", // if
        "senão", // else
        "enquanto", // while
        "para", // for
        "repita", // do
        "retorne", // return
        "quebra" // break
    };

    public static bool IsKeyword(string s)
    {
        for (int i = 0; i < KEYWORDS.Length; i++)
        {
            if (s == KEYWORDS[i])
                return true;
        }

        return false;
    }

    public string Value
    {
        get;
    }

    internal Keyword(SourceInterval interval, string value) : base(interval)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"keyword '{Value}'";
    }
}
