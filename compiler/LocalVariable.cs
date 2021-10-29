using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class LocalVariable : Variable
    {
        private Function function;

        public Function Function
        {
            get
            {
                return function;
            }
        }

        public LocalVariable(Function function, string name, AbstractType type, int offset) : base(name, type, offset)
        {
            this.function = function;
        }
    }
}
