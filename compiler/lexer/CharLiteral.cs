using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.lexer
{
    public class CharLiteral : Literal
    {
        private char value;

        public char Value => value;

        internal CharLiteral(SourceInterval interval, char value) : base(interval)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
