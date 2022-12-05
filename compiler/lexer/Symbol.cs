namespace compiler.lexer
{
    public class Symbol : Token
    {
        public static readonly string[] SYMBOLS = { 
            // operadores aritiméticos
            "+", 
            "-", 
            "*", 
            "/",
            "%",
            // operadores bit a bit
            "&", 
            "|", 
            "^",
            "~", 
            // operadores de incremento e decremento
            "++",
            "--", 
            // operadores relacionais
            "==",
            "!=",
            ">",
            "<",
            ">=",
            "<=",
            // operadores lógicos
            "&&" , 
            "||", 
            "^^", 
            "!", 
            // operadores de deslocamento
            "<<", 
            ">>", 
            ">>>",
            // operadores de atribuição
            "=",
            "+=",
            "-=",
            "*=",
            "/=",
            "%=",
            "&=",
            "|=",
            "^=",
            "<<=",
            ">>=",
            ">>>=",
            // outros
            "(",
            ")",
            "[",
            "]",
            "{",
            "}",
            ",",
            ".",
            ":",
            ";"
        };

        public static bool IsSymbol(string s)
        {
            for (int i = 0; i < SYMBOLS.Length; i++)
            {
                if (s == SYMBOLS[i])
                    return true;
            }

            return false;
        }

        public static bool IsSymbol(char c) => IsSymbol(c.ToString());

        public string Value
        {
            get;
        }

        internal Symbol(SourceInterval interval, string value) : base(interval) => Value = value;

        public override string ToString() => $"symbol '{Value}'";
    }
}
