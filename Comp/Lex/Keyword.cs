namespace Comp.Lex;

public class Keyword : Token
{
    public static readonly string[] KEYWORDS = [ 
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
        "publico",
        "protegido",
        "privado",
        "função",
        "esterna",
        "estrutura",
        "classe",
        "interface",
        "enum",
        "união",
        "estatico",
        "dinamico",
        "virtual",
        "abstrato",
        "sobreposto", // override
        "programa",
        "unidade",
        "usando",

        // leitura e escrita
        "leia",
        "escreva",
        "escrevaln",

        // literais
        "verdade", // true
        "falso", // false
        "isto", // this
        "nulo", // null

        // estruturas de controle de fluxo
        "se", // if
        "senão", // else
        "enquanto", // while
        "para", // for
        "repita", // do
        "retorne", // return
        "quebra", // break
        "continue",
        "lance", // throw
        "tente", // try
        "capture", // catch
        "finalmente" // finally
    ];

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
