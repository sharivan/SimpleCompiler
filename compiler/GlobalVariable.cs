using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class GlobalVariable : Variable
    {
        public GlobalVariable(string name, AbstractType type, int offset) : base(name, type, offset) { }
    }
}
