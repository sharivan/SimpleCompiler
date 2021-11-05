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
        private int offset;
        private Stack<Label> breakLabels;

        public Function Function
        {
            get
            {
                return function;
            }
        }

        public Context Parent
        {
            get
            {
                return parent;
            }
        }

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
            breakLabels = new Stack<Label>();

            if (parent == null)
                AddParams(function);
        }

        public bool IsRoot()
        {
            return parent == null;
        }

        public LocalVariable DeclareLocalVariable(Function function, string name, AbstractType type, bool recursive = true)
        {
            for (int i = 0; i < variables.Count; i++)
                if (variables[i].Name == name)
                    return null;

            if (recursive && parent != null && parent.FindVariable(name, true) != null)
                return null;

            LocalVariable result = new LocalVariable(function, name, type, RealOffset);
            offset += type.Size();
            function.CheckLocalVariableOffset(offset);
            variables.Add(result);
            return result;
        }

        public void AddParams(Function function)
        {
            if (function == null)
                return;

            for (int i = 0; i < function.ParamCount; i++)
            {
                Parameter parameter = function[i];
                variables.Add(parameter);
            }
        }

        public Variable FindVariable(string name, bool recursive = true)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                Variable var = variables[i];
                if (var.Name == name)
                    return var;
            }

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
