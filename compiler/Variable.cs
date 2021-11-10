using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;

namespace compiler
{
    public abstract class Variable
    {
        private string name;
        private AbstractType type;
        private int offset;

        public string Name => name;

        public AbstractType Type => type;

        public int Offset
        {
            get => offset;

            internal set => offset = value;
        }

        protected Variable(string name, AbstractType type, int offset)
        {
            this.name = name;
            this.type = type;
            this.offset = offset;
        }

        public override string ToString()
        {
            return name + ":" + type + " (" + offset + ")";
        }
    }
}
