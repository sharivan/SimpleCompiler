using Asm;
using Comp.Types;
using System;
using System.Collections.Generic;

namespace Comp;

public class Function : IMember
{
    private readonly List<Parameter> parameters;
    private AbstractType returnType;

    private List<TypeEntry> types;

    public CompilationUnity Unity
    {
        get;
    }

    public string Name
    {
        get;
    }

    public SourceInterval Interval
    {
        get;
        internal set;
    }

    public bool IsExtern
    {
        get;
    }

    public Parameter this[int index] => parameters[index];

    public IEnumerable<Parameter> Parameters => parameters;

    public int ParamCount => parameters.Count;

    public AbstractType ReturnType
    {
        get => returnType;

        internal set => returnType = value;
    }

    public BlockStatement Block
    {
        get;
        internal set;
    }

    public int ParameterOffset
    {
        get;
        private set;
    }

    public int ParameterSize
    {
        get;
        private set;
    }

    public int LocalVariableOffset
    {
        get;
        private set;
    }

    public int ReturnOffset
    {
        get;
        private set;
    }

    public Label EntryLabel
    {
        get;
        private set;
    }

    public Label ReturnLabel
    {
        get;
        private set;
    }

    public FieldAggregationType DeclaringType
    {
        get;
    }

    internal Function(CompilationUnity unity, FieldAggregationType declaringType, string name, SourceInterval interval, bool isExtern = false)
    {
        Unity = unity;
        DeclaringType = declaringType;
        Name = name;
        Interval = interval;
        IsExtern = isExtern;

        parameters = [];
        returnType = PrimitiveType.VOID;
        ParameterOffset = -2 * sizeof(int);
        ParameterSize = 0;
        LocalVariableOffset = 0;
        ReturnOffset = -2 * sizeof(int);

        types = [];
    }

    internal int AcquireFreeOffset(AbstractType type, int fromOffset = 0, bool tempVar = false)
    {
        int offset = 0;
        foreach (var entry in types)
        {
            if (!entry.acquired && entry.tempVar == tempVar && entry.offset >= fromOffset && ReferenceEquals(entry.type, type))
            {
                entry.acquired = true;
                return entry.offset;
            }

            offset += Compiler.GetAlignedSize(entry.type.Size);
        }

        types.Add(new TypeEntry(offset, type, true, tempVar));
        LocalVariableOffset = offset + Compiler.GetAlignedSize(type.Size);
        return offset;
    }

    internal bool ReleaseOffset(int offset)
    {
        foreach (var entry in types)
        {
            if (entry.acquired && entry.offset == offset)
            {
                entry.acquired = false;
                return true;
            }
        }

        return false;
    }

    internal void CreateEntryLabel()
    {
        EntryLabel = Unity.Compiler.CreateLabel();
    }

    internal void CreateReturnLabel()
    {
        ReturnLabel = Unity.Compiler.CreateLabel();
    }

    internal void BindEntryLabel(Assembler assembler)
    {
        assembler.BindLabel(EntryLabel);
    }

    internal void BindReturnLabel(Assembler assembler)
    {
        assembler.BindLabel(ReturnLabel);
    }

    internal void BeginBlock(Assembler assembler)
    {
        if (LocalVariableOffset > 0)
            assembler.EmitAddSP(LocalVariableOffset);
    }

    internal void EndBlock(Assembler assembler)
    {
        assembler.AddLine(Interval.FileName, Interval.LastLine);

        foreach (var p in parameters)
        {
            var type = p.Type;
            if (type is StringType)
            {
                var f = Unity.Compiler.unitySystem.FindFunction("DecrementaReferenciaString");
                int index = Unity.Compiler.GetOrAddExternalFunction(f.Name, f.ParameterSize);
                assembler.EmitLoadLocalHostAddress(p.Offset);
                assembler.EmitExternCall(index);
            }
        }

        if (LocalVariableOffset > 0)
            assembler.EmitSubSP(LocalVariableOffset);

        int count = -ParameterOffset - 2 * sizeof(int);
        if (count > 0)
            assembler.EmitRetN(count);
        else
            assembler.EmitRet();
    }

    public Parameter FindParameter(string name)
    {
        foreach (var p in parameters)
        {
            if (p.Name == name)
                return p;
        }

        return null;
    }

    internal Parameter DeclareParameter(string name, AbstractType type, SourceInterval interval, bool byRef)
    {
        var result = FindParameter(name);
        if (result != null)
            return null;

        result = new Parameter(this, name, type, interval, ParameterOffset, byRef);
        parameters.Add(result);

        return result;
    }

    internal void Resolve()
    {
        ParameterOffset = -2 * sizeof(int);
        ParameterSize = 0;
        for (int i = parameters.Count - 1; i >= 0; i--)
        {
            var parameter = parameters[i];
            parameter.Resolve();
            var paramType = parameter.Type;
            int size = parameter.ByRef ? IntPtr.Size : Compiler.GetAlignedSize(paramType.Size);
            ParameterOffset -= size;
            parameter.Offset = ParameterOffset;
            ParameterSize += size;
        }

        ReturnOffset = ParameterOffset;
        AbstractType.Resolve(ref returnType);
        if (!PrimitiveType.IsPrimitiveVoid(returnType))
            ReturnOffset -= Compiler.GetAlignedSize(returnType.Size);
    }
}