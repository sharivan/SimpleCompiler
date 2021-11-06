using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public abstract class Literal : Token
    {
        public Literal(SourceInterval interval) : base(interval)
        {
        }
    }
}
