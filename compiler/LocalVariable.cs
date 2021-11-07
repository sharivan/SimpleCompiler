using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;

namespace compiler
{
    public class LocalVariable : Variable
    {
        private Function function;

        public Function Function => function;

        private bool Param => Offset < 0;

        public LocalVariable(Function function, string name, AbstractType type, int offset) : base(name, type, offset)
        {
            this.function = function;
        }
    }
}
