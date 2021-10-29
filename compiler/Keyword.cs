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
            // declarações
            "declare",
            "função",
            "estrutura",
            "programa",
            // leitura e escrita
            "leia",
            "escreva",
            // literais lógicos
            "verdadeiro", // true
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

        public string Value
        {
            get
            {
                return value;
            }
        }

        public Keyword(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "keyword '" + value + "'";
        }
    }
}
