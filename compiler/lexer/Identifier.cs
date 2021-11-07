using System;
using System.Collections.Generic;
using System.Text;

namespace compiler.lexer
{
    public class Identifier : Token
    {
        public static bool IsLetter(char c)
        {
            return 'A' <= c && c <= 'Z' ||
                'a' <= c && c <= 'z' ||
                c != '\u00d7' && '\u00c0' <= c && c <= '\u00dd' ||
                c != '\u00f7' && '\u00e0' <= c && c <= '\u00ff';
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

        private string name;

        public string Name => name;

        public Identifier(SourceInterval interval, string name) : base(interval)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return "identifier '" + name + "'";
        }
    }
}
