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

        public bool Param => Offset < 0;

        internal LocalVariable(Function function, string name, AbstractType type, SourceInterval interval, int offset = -1) : base(name, type, interval, offset)
        {
            this.function = function;
        }
    }
}
