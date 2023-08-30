using System.Collections.Generic;

using Asm;

namespace Comp.Types;

public class StructType : NamedType
{
    private readonly List<Field> fields;
    private int size;

    public int FieldAlignSize
    {
        get;
    }

    public int FieldCount => fields.Count;

    public Field this[int index] => fields[index];

    internal StructType(CompilationUnity unity, string name, SourceInterval interval, int fieldAlignSize = sizeof(byte)) : base(unity, name, interval)
    {
        FieldAlignSize = fieldAlignSize;

        fields = new List<Field>();

        size = 0;
    }

    public Field FindField(string name)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            Field field = fields[i];
            if (field.Name == name)
                return field;
        }

        return null;
    }

    internal Field DeclareField(string name, AbstractType type, SourceInterval interval)
    {
        Field result = FindField(name);
        if (result != null)
            return null;

        result = new Field(this, name, type, interval);
        fields.Add(result);
        return result;
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
        size = 0;
        foreach (var field in fields)
        {
            field.Resolve();
            AbstractType type = field.Type;

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

        foreach (Field field in fields)
            result += "  " + field + "\n";

        return result + "}";
    }

    protected override int GetSize()
    {
        return size;
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        return Equals(other);
    }

    public override int GetHashCode()
    {
        int hashCode = -1735305858;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Field>>.Default.GetHashCode(fields);
        return hashCode;
    }

    public override bool IsUnresolved()
    {
        return false;
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

    internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
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