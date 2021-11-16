using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.types
{
    public class TypeSetType : NamedType
    {
        private AbstractType type;

        public AbstractType Type => type;

        internal TypeSetType(CompilationUnity unity, string name, AbstractType type, SourceInterval interval) : base(unity, name, interval)
        {
            this.type = type;
        }

        public override bool CoerceWith(AbstractType other, bool isExplicit)
        {
            return type.CoerceWith(other, isExplicit);
        }

        public override int Size()
        {
            return type.Size();
        }

        public override bool IsUnresolved()
        {
            return type.IsUnresolved();
        }

        internal void Resolve()
        {
            if (!resolved)
            {
                resolved = true;
                UncheckedResolve();
            }
        }

        protected override void UncheckedResolve()
        {
            if (type is UnresolvedType u)
            {
                if (u.ReferencedType == null)
                    throw new CompilerException(u.Interval, "Tipo não declarado '" + u.Name + "'.");

                type = u.ReferencedType;
            }
            else
                Resolve(ref type);
        }
    }
}
