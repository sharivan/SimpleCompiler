using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.lexer
{
    public abstract class Literal : Token
    {
        protected Literal(SourceInterval interval) : base(interval)
        {
        }
    }
}
