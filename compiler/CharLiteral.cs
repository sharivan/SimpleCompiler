using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class CharLiteral : Literal
    {
        private char value;

        public char Value
        {
            get
            {
                return value;
            }
        }

        public CharLiteral(char value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
