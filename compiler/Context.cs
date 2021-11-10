using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;
using assembler;

namespace compiler
{
    public class Context
    {
        private Function function;
        private Context parent;
        private List<Variable> variables;
        private Dictionary<string, Variable> variableTable;
        private int offset;
        private Stack<Label> breakLabels;

        public Function Function => function;

        public Context Parent => parent;

        public int RealOffset
        {
            get
            {
                int result = offset;
                if (parent != null)
                    result += parent.RealOffset;

                return result;
            }
        }

        internal Context(Function function, Context parent = null)
        {
            this.function = function;
            this.parent = parent;
            
            variables = new List<Variable>();
            variableTable = new Dictionary<string, Variable>();
            breakLabels = new Stack<Label>();

            offset = 0;

            if (parent == null)
                AddParams(function);
        }

        public bool IsRoot()
        {
            return parent == null;
        }

        internal Variable DeclareLocalVariable(Function function, string name, AbstractType type, bool recursive = true)
        {
            if (variableTable.TryGetValue(name, out Variable result))
                return result;

            if (recursive && parent != null && parent.FindVariable(name, true) != null)
                return null;

            result = new LocalVariable(function, name, type, RealOffset);
            offset += Compiler.GetAlignedSize(type.Size());
            function.CheckLocalVariableOffset(RealOffset);
            variables.Add(result);
            variableTable.Add(name, result);
            return result;
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
            if (variableTable.TryGetValue(name, out Variable result))
                return result;

            if (recursive && parent != null)
                return parent.FindVariable(name, true);

            return null;
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
            if (breakLabels.Count == 0)
            {
                if (parent != null)
                    return parent.FindNearestBreakLabel();

                return null;
            }

            return breakLabels.Pop();
        }
    }
}
