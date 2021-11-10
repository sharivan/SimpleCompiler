using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;

namespace compiler
{
    public class Parameter : Variable
    {
        private Function function;
        private bool byRef;

        public Function Function => function;

        public bool ByRef => byRef;

        internal Parameter(Function function, string name, AbstractType type, int offset, bool byRef) : base(name, type, offset)
        {
            this.function = function;
            this.byRef = byRef;
        }

        public override string ToString()
        {
            return (byRef ? "&" : "") + base.ToString();
        }
    }
}
