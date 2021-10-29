using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class PointerType : AbstractType
    {
        private AbstractType type;

        public AbstractType Type
        {
            get
            {
                return type;
            }
        }

        public PointerType(AbstractType type)
        {
            this.type = type;
        }

        public override string ToString()
        {
            return "*" + (type != null ? type.ToString() : "void");
        }

        public override int Size()
        {
            return 4;
        }
    }
}
