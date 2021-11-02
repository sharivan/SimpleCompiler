using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public class Identifier : Token
    {
        public static bool IsLetter(char c)
        {
            return 'A' <= c && c <= 'Z' || 
                'a' <= c && c <= 'z' ||
                c == 'Ç' ||
                c == 'Ã' ||
                c == 'Õ' ||
                c == 'Á' ||
                c == 'É' ||
                c == 'Í' ||
                c == 'Ó' ||
                c == 'Ú' ||
                c == 'ç' || 
                c == 'ã' || 
                c == 'õ' || 
                c == 'á' || 
                c == 'é' || 
                c == 'í' || 
                c == 'ó' ||
                c == 'ú';
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

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Identifier(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return "identifier '" + name + "'";
        }
    }
}
