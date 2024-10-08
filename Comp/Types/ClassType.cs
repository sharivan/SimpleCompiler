﻿using Asm;
using System.Collections.Generic;

namespace Comp.Types;

public class ClassType : FieldAggregationType
{
    protected readonly List<Function> methods;

    public ClassType SuperClass
    {
        get;
        internal set;
    }

    public bool IsInterface
    {
        get;
        internal set;
    }

    internal ClassType(CompilationUnity unity, string name, SourceInterval interval, int fieldAlignSize = sizeof(byte))
        : base(unity, name, interval, fieldAlignSize)
    {
        methods = [];
    }

    public override string ToString()
    {
        string result = (IsInterface ? "interface " : "classe ") + Name + "\n{\n";

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
        return false;
    }

    protected override void UncheckedResolve()
    {
        size = 0;
        foreach (var field in fields)
        {
            field.Resolve();

            var type = field.Type;
            field.Offset = this.size;
            int size = type.Size;
            this.size += Compiler.GetAlignedSize(size, FieldAlignSize);
        }
    }

    protected internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
    }
}