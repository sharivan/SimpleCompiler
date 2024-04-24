using Asm;

namespace Comp.Types;

public class StructType : FieldAggregationType
{
    internal StructType(CompilationUnity unity, string name, SourceInterval interval, int fieldAlignSize = sizeof(byte))
        : base(unity, name, interval, fieldAlignSize)
    {
    }

    protected override void UncheckedResolve()
    {
        size = 0;
        foreach (var field in fields)
        {
            field.Resolve();
            var type = field.Type;

            switch (type)
            {
                case ArrayType t:
                {
                    if (t.Type is StructType st && st == this)
                        throw new CompilerException(field.Interval, "Uma estrutura não pode conter um tipo de campo que faz referência direta a ela mesma.");

                    break;
                }

                case StructType t:
                {
                    if (t == this)
                        throw new CompilerException(field.Interval, "Uma estrutura não pode conter um tipo de campo que faz referência direta a ela mesma.");

                    break;
                }

                case TypeSetType t:
                {
                    if (t.Type is StructType st && st == this)
                        throw new CompilerException(field.Interval, "Uma estrutura não pode conter um tipo de campo que faz referência direta a ela mesma.");

                    break;
                }
            }

            field.Offset = this.size;
            int size = type.Size;
            this.size += Compiler.GetAlignedSize(size, FieldAlignSize);
        }
    }

    public override string ToString()
    {
        string result = "estrutura " + Name + "\n{\n";

        foreach (var field in fields)
            result += "  " + field + "\n";

        return result + "}";
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        return Equals(other);
    }

    public override bool ContainsString()
    {
        foreach (var field in fields)
        {
            if (field.Type.ContainsString())
                return true;
        }

        return false;
    }

    protected internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
        int fieldCount = 0;
        foreach (var field in fields)
        {
            if (field.Type.ContainsString())
                fieldCount++;
        }

        if (fieldCount == 0)
            return;

        int counter = 0;
        foreach (var field in fields)
        {
            if (field.Type.ContainsString())
            {
                if (releaseType == ReleaseType.PTR)
                {
                    if (counter < fieldCount)
                        assembler.EmitDupPtr();

                    if (offset != 0)
                    {
                        assembler.EmitLoadConst(offset);
                        assembler.EmitPtrAdd();
                    }
                }

                field.Type.EmitStringRelease(context, compiler, assembler, offset + field.Offset, releaseType);
                counter++;
            }
        }
    }
}