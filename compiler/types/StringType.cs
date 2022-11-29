using System;

namespace compiler.types
{
#pragma warning disable CS0659 // O tipo substitui Object. Equals (objeto o), mas não substitui o Object.GetHashCode()
    public class StringType : AbstractType
#pragma warning restore CS0659 // O tipo substitui Object. Equals (objeto o), mas não substitui o Object.GetHashCode()
    {
        public static readonly StringType STRING = new();

        private StringType()
        { 
        }

        public override bool CoerceWith(AbstractType other, bool isExplicit) => other is ArrayType a
                ? isExplicit && a.Type == PrimitiveType.CHAR
                : other is PointerType p && (p.Type == PrimitiveType.CHAR || p.Type == PrimitiveType.VOID && isExplicit);

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj != null && obj is StringType;

        public override bool IsUnresolved() => false;

        protected override int GetSize() => IntPtr.Size;

        protected override void UncheckedResolve()
        {
        }
    }
}
