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

        internal Field(StructType container, string name, AbstractType type, SourceInterval interval, int offset = -1) : base(name, type, interval, offset)
        {           
            this.container = container;
        }
    }
}
