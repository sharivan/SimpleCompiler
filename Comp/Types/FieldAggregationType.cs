using System.Collections.Generic;

namespace Comp.Types;

public abstract class FieldAggregationType : NamedType
{
    protected readonly List<Field> fields;
    protected int size;

    public int FieldAlignSize
    {
        get;
    }

    public int FieldCount => fields.Count;

    public Field this[int index] => fields[index];

    internal FieldAggregationType(CompilationUnity unity, string name, SourceInterval interval, int fieldAlignSize = sizeof(byte))
        : base(unity, name, interval)
    {
        FieldAlignSize = fieldAlignSize;

        fields = [];

        size = 0;
    }

    public Field FindField(string name)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            if (field.Name == name)
                return field;
        }

        return null;
    }

    internal Field DeclareField(string name, AbstractType type, SourceInterval interval)
    {
        var result = FindField(name);
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

    protected override int GetSize()
    {
        return size;
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
}