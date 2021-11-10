using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;

namespace compiler
{
    public class Field : Variable
    {
        private StructType container;

        public StructType Container => container;

        internal Field(StructType container, string name, AbstractType type, int offset) : base(name, type, offset)
        {           
            this.container = container;
        }
    }
}
