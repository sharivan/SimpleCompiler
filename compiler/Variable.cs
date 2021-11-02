using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public abstract class Variable
    {
        private string name;
        private AbstractType type;
        private int offset;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public AbstractType Type
        {
            get
            {
                return type;
            }
        }

        public int Offset
        {
            get
            {
                return offset;
            }

            set
            {
                offset = value;
            }
        }

        public Variable(string name, AbstractType type, int offset)
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
