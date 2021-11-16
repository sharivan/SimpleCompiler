using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.types
{
    public class UnresolvedType : NamedType
    {
        private AbstractType referencedType;

        public AbstractType ReferencedType
        {
            get => referencedType;

            internal set => referencedType = value;
        }

        internal UnresolvedType(CompilationUnity unity, string name, SourceInterval interval) : base(unity, name, interval)
        {
        }

        public override bool CoerceWith(AbstractType other, bool isExplicit)
        {
            throw new NotImplementedException();
        }

        public override int Size()
        {
            throw new NotImplementedException();
        }

        public override bool IsUnresolved()
        {
            return referencedType == null;
        }

        protected override void UncheckedResolve()
        {
            if (referencedType == null)
            {
                referencedType = Unity.FindStruct(Name);
                if (referencedType == null)
                    throw new CompilerException(Interval, "Tipo não declarado '" + Name + "'.");

                Resolve(ref referencedType);
            }
        }
    }
}
