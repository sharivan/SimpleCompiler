using System.Collections.Generic;

using assembler;

using compiler.types;

namespace compiler;

public class Context
{
    private List<Variable> variables;
    private List<LocalVariable> temporaryVariables;
    private Dictionary<string, Variable> variableTable;
    private int offset;
    private Stack<Label> breakLabels;

    public CompilationUnity Unity
    {
        get;
    }

    public Function Function
    {
        get;
    }

    public SourceInterval Interval
    {
        get;
    }

    public Context Parent
    {
        get;
    }

    public int RealOffset
    {
        get
        {
            int result = offset;
            if (Parent != null)
                result += Parent.RealOffset;

            return result;
        }
    }

    internal Context(Function function, SourceInterval interval, Context parent = null)
    {
        Function = function;
        Unity = function.Unity;
        Interval = interval;
        Parent = parent;

        Initalize();
    }

    internal Context(CompilationUnity unity, SourceInterval interval, Context parent = null)
    {
        Function = null;
        Unity = unity;
        Interval = interval;
        Parent = parent;

        Initalize();
    }

    private void Initalize()
    {
        variables = new List<Variable>();
        temporaryVariables = new List<LocalVariable>();
        variableTable = new Dictionary<string, Variable>();
        breakLabels = new Stack<Label>();

        offset = 0;

        if (Parent == null && Function != null)
            AddParams(Function);
    }

    public bool IsRoot()
    {
        return Parent == null;
    }

    internal Variable DeclareVariable(string name, AbstractType type, SourceInterval interval, bool recursive = true)
    {
        if (variableTable.TryGetValue(name, out Variable result))
            return result;

        if (recursive && Parent != null && Parent.FindVariable(name, true) != null)
            return null;

        if (Function != null)
        {
            result = new LocalVariable(Function, name, type, interval, RealOffset);
            offset += Compiler.GetAlignedSize(type.Size);
            Function.CheckLocalVariableOffset(RealOffset);
        }
        else
        {
            result = Unity.DeclareGlobalVariable(name, type, interval);
            offset += Compiler.GetAlignedSize(type.Size);
        }

        variables.Add(result);
        variableTable.Add(name, result);
        return result;
    }

    internal Variable DeclareTemporaryVariable(AbstractType type, SourceInterval interval)
    {
        var tempVar = DeclareVariable($"@tempvar_{type}_{temporaryVariables.Count}", type, interval, false);
        tempVar.Temporary = true;
        return tempVar;
    }

    internal Variable AcquireTemporaryVariable(AbstractType type, SourceInterval interval)
    {
        foreach (var tempVar in temporaryVariables)
        {
            if (!tempVar.Acquired && tempVar.Type == type)
            {
                tempVar.Acquired = true;
                return tempVar;
            }
        }

        Variable tempVar2 = DeclareTemporaryVariable(type, interval);
        tempVar2.Acquired = true;
        return tempVar2;
    }

    internal void AddParams(Function function)
    {
        for (int i = 0; i < function.ParamCount; i++)
        {
            Parameter parameter = function[i];
            variables.Add(parameter);
            variableTable.Add(parameter.Name, parameter);
        }
    }

    public Variable FindVariable(string name, bool recursive = true)
    {
        return variableTable.TryGetValue(name, out Variable result)
            ? result
            : recursive && Parent != null ? Parent.FindVariable(name, true) : null;
    }

    internal void PushBreakLabel(Label label)
    {
        breakLabels.Push(label);
    }

    internal void DropBreakLabel()
    {
        breakLabels.Pop();
    }

    public Label FindNearestBreakLabel()
    {
        return breakLabels.Count == 0 ? Parent?.FindNearestBreakLabel() : breakLabels.Pop();
    }

    public void Release(Assembler assembler)
    {
        assembler.AddLine(Interval.FileName, Interval.LasttLine);

        Compiler comp = Unity.Compiler;
        for (int i = 0; i < variables.Count; i++)
        {
            var v = variables[i];
            AbstractType.ReleaseType releaseType;
            if (v is LocalVariable local)
            {
                releaseType = AbstractType.ReleaseType.LOCAL;
                local.Scope = SourceInterval.Append(local.Scope, Interval);
            }
            else
            {
                releaseType = AbstractType.ReleaseType.GLOBAL;
            }

            AbstractType type = v.Type;
            type.EmitStringRelease(this, comp, assembler, v.Offset, releaseType);
        }
    }
}
