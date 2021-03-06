using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.lexer
{
    public class StringLiteral : Literal
    {
        private string value;

        public string Value => value;

        internal StringLiteral(SourceInterval interval, string value) : base(interval)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
