using System.Collections.Generic;

using compiler.types;
using assembler;

namespace compiler
{
    public class Context
    {
        private readonly List<Variable> variables;
        private readonly List<LocalVariable> temporaryVariables;
        private readonly Dictionary<string, Variable> variableTable;
        private int offset;
        private readonly Stack<Label> breakLabels;

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
            Interval = interval;
            Parent = parent;
            
            variables = new List<Variable>();
            temporaryVariables = new List<LocalVariable>();
            variableTable = new Dictionary<string, Variable>();
            breakLabels = new Stack<Label>();

            offset = 0;

            if (parent == null)
                AddParams(function);
        }

        public bool IsRoot() => Parent == null;

        internal Variable DeclareLocalVariable(Function function, string name, AbstractType type, SourceInterval interval, bool recursive = true)
        {
            if (variableTable.TryGetValue(name, out Variable result))
                return result;

            if (recursive && Parent != null && Parent.FindVariable(name, true) != null)
                return null;

            result = new LocalVariable(function, name, type, interval, RealOffset);
            offset += Compiler.GetAlignedSize(type.Size);
            function.CheckLocalVariableOffset(RealOffset);
            variables.Add(result);
            variableTable.Add(name, result);
            return result;
        }

        internal LocalVariable DeclareTemporaryVariable(Function function, AbstractType type, SourceInterval interval)
        {
            var tempVar = (LocalVariable) DeclareLocalVariable(function, "@tempvar" + temporaryVariables.Count, type, interval);
            tempVar.Temporary = true;
            return tempVar;
        }

        internal LocalVariable AcquireTemporaryVariable(Function function, AbstractType type, SourceInterval interval)
        {
            foreach (var tempVar in temporaryVariables)
            {
                if (!tempVar.Acquired && tempVar.Type == type)
                {
                    tempVar.Acquired = true;
                    return tempVar;
                }
            }

            LocalVariable tempVar2 = DeclareTemporaryVariable(function, type, interval);
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

        public Variable FindVariable(string name, bool recursive = true) => variableTable.TryGetValue(name, out Variable result)
                ? result
                : recursive && Parent != null ? Parent.FindVariable(name, true) : null;

        internal void PushBreakLabel(Label label) => breakLabels.Push(label);

        internal void DropBreakLabel() => breakLabels.Pop();

        public Label FindNearestBreakLabel() => breakLabels.Count == 0 ? Parent?.FindNearestBreakLabel() : breakLabels.Pop();

        public void Release(Assembler assembler)
        {
            assembler.AddLine(Interval.FileName, Interval.LasttLine);

            Compiler comp = Function.Unity.Compiler;
            foreach (Variable v in variables)
            {
                AbstractType type = v.Type;
                if (type is StringType)
                {
                    Function f = comp.unitySystem.FindFunction("DecrementaReferenciaTexto");
                    int index = comp.GetOrAddExternalFunction(f.Name, f.ParameterSize);
                    assembler.EmitLoadLocalHostAddress(v.Offset);
                    assembler.EmitExternCall(index);
                }
            }
        }
    }
}
