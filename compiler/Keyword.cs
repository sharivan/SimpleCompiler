using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
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
            // conversão de tipo
            "cast",
            // declarações
            "var",
            "função",
            "estrutura",
            "programa",
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

        private string value;

        public string Value => value;

        public Keyword(SourceInterval interval, string value) : base(interval)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "keyword '" + value + "'";
        }
    }
}
