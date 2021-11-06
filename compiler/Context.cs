using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Context(Function function, Context parent = null)
        {
            this.function = function;
            this.parent = parent;
            
            variables = new List<Variable>();
            variableTable = new Dictionary<string, Variable>();
            breakLabels = new Stack<Label>();

            if (parent == null)
                AddParams(function);
        }

        public bool IsRoot()
        {
            return parent == null;
        }

        public Variable DeclareLocalVariable(Function function, string name, AbstractType type, bool recursive = true)
        {
            if (variableTable.TryGetValue(name, out Variable result))
                return result;

            if (recursive && parent != null && parent.FindVariable(name, true) != null)
                return null;

            result = new LocalVariable(function, name, type, RealOffset);
            offset += type.Size();
            function.CheckLocalVariableOffset(offset);
            variables.Add(result);
            variableTable.Add(name, result);
            return result;
        }

        public void AddParams(Function function)
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

        public void PushBreakLabel(Label label)
        {
            breakLabels.Push(label);
        }

        public void DropBreakLabel()
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
