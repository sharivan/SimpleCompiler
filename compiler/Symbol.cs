using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
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

        public static bool IsSymbol(char c)
        {
            return IsSymbol(c.ToString());
        }

        private string value;

        public string Value
        {
            get
            {
                return value;
            }
        }

        public Symbol(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "symbol '" + value + "'";
        }
    }
}
