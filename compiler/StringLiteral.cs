using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class StringLiteral : Literal
    {
        private string value;

        public string Value
        {
            get
            {
                return value;
            }
        }

        public StringLiteral(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
