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
        private SourceInterval interval;
        private int offset;

        public string Name => name;

        public AbstractType Type => type;

        public SourceInterval Interval => interval;

        public int Offset
        {
            get => offset;

            internal set => offset = value;
        }

        protected Variable(string name, AbstractType type, SourceInterval interval, int offset = -1)
        {
            this.name = name;
            this.type = type;
            this.interval = interval;
            this.offset = offset;
        }

        public override string ToString()
        {
            return name + ":" + type + " (" + offset + ")";
        }

        internal void Resolve()
        {
            AbstractType.Resolve(ref type);
        }
    }
}
