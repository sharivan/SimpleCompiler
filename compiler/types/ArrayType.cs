using System.Collections.Generic;

using assembler;

namespace compiler.types;

public class ArrayType : AbstractType
{
    private AbstractType type;
    private readonly List<int> boundaries;

    public AbstractType Type => type;

    public int Rank => boundaries.Count;

    public int this[int index] => boundaries[index];

    internal ArrayType(AbstractType type)
    {
        this.type = type;

        boundaries = new List<int>();
    }

    internal void AddBoundary(int boundary)
    {
        boundaries.Add(boundary);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj == null)
            return false;

        if (obj is ArrayType a)
        {
            AbstractType otherType = a.Type;
            if (type != otherType)
                return false;

            if (boundaries.Count != a.boundaries.Count)
                return false;

            for (int i = 0; i < boundaries.Count; i++)
            {
                if (boundaries[i] != a.boundaries[i])
                    return false;
            }

            return true;
        }

        return false;
    }

    public override string ToString()
    {
        string result = type + "[";
        if (boundaries.Count > 0)
        {
            result += boundaries[0].ToString();
            for (int i = 1; i < boundaries.Count; i++)
                result += ", " + boundaries[i];
        }

        return result + "]";
    }

    protected override int GetSize()
    {
        return GetLength() * type.Size;
    }

    public int GetLength()
    {
        if (boundaries.Count == 0)
            return 0;

        int result = 1;
        foreach (int bondary in boundaries)
            result *= bondary;

        return result;
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        return Equals(other);
    }

    public override int GetHashCode()
    {
        int hashCode = 13823892;
        hashCode = hashCode * -1521134295 + EqualityComparer<AbstractType>.Default.GetHashCode(type);
        hashCode = hashCode * -1521134295 + EqualityComparer<List<int>>.Default.GetHashCode(boundaries);
        return hashCode;
    }

    public static bool operator ==(ArrayType t1, ArrayType t2)
    {
        return ReferenceEquals(t1, t2) || t1 is not null && t2 is not null && t1.Equals(t2);
    }

    public static bool operator !=(ArrayType t1, ArrayType t2)
    {
        return !(t1 == t2);
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
                throw new CompilerException(u.Interval, $"Tipo não declarado: '{u.Name}'.");

            type = u.ReferencedType;
        }
        else
        {
            Resolve(ref type);
        }
    }

    public override bool ContainsString()
    {
        return type.ContainsString();
    }

    internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
        if (type is StringType)
        {
            Function f = compiler.unitySystem.FindFunction("DecrementaReferenciaArrayTexto");
            int index = compiler.GetOrAddExternalFunction(f.Name, f.ParameterSize);

            switch (releaseType)
            {
                case ReleaseType.GLOBAL:
                    assembler.EmitLoadGlobalHostAddress(offset);
                    break;

                case ReleaseType.LOCAL:
                    assembler.EmitLoadLocalHostAddress(offset);
                    break;

                case ReleaseType.PTR:
                    if (offset != 0)
                    {
                        assembler.EmitLoadConst(offset);
                        assembler.EmitPtrAdd();
                    }

                    break;
            }

            assembler.EmitLoadConst(GetLength());
            assembler.EmitLoadConst(true);
            assembler.EmitExternCall(index);
        }
        else if (type.ContainsString())
        {
            Variable counter = context.DeclareTemporaryVariable(PrimitiveType.INT, context.Interval);
            assembler.EmitLoadConst(0);
            compiler.CompileStore(assembler, null, counter, context.Interval);

            Label lblLoop = compiler.CreateLabel();
            assembler.BindLabel(lblLoop);
            compiler.CompileLoad(assembler, counter, context.Interval);
            assembler.EmitLoadConst(GetLength());
            assembler.EmitCompareLess();
            Label lblEnd = compiler.CreateLabel();
            assembler.EmitJumpIfFalse(lblEnd);

            switch (releaseType)
            {
                case ReleaseType.GLOBAL:
                    assembler.EmitLoadGlobalHostAddress(offset);
                    break;

                case ReleaseType.LOCAL:
                    assembler.EmitLoadLocalHostAddress(offset);
                    break;

                case ReleaseType.PTR:
                    if (offset != 0)
                    {
                        assembler.EmitLoadConst(offset);
                        assembler.EmitPtrAdd();
                    }

                    break;
            }

            compiler.CompileLoad(assembler, counter, context.Interval);
            assembler.EmitLoadConst(type.Size);
            assembler.EmitMul();
            assembler.EmitPtrAdd();
            type.EmitStringRelease(context, compiler, assembler, 0, ReleaseType.PTR);

            compiler.CompileLoad(assembler, counter, context.Interval);
            assembler.EmitLoadConst(1);
            compiler.CompileStoreAdd(assembler, null, counter, context.Interval);
            assembler.EmitJump(lblLoop);
            assembler.BindLabel(lblEnd);
        }
    }
}
