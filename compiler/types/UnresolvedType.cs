using System;

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

        public override bool CoerceWith(AbstractType other, bool isExplicit) => throw new NotImplementedException();

        protected override int GetSize() => throw new NotImplementedException();

        public override bool IsUnresolved() => referencedType == null;

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
