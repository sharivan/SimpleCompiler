using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public abstract class NumericLiteral : Token
    {
        public abstract byte AsByte();

        public abstract short AsShort();

        public abstract int AsInt();

        public abstract long AsLong();

        public abstract float AsFloat();

        public abstract double AsDouble();
    }
}
