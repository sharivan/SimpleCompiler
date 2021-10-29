using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class Function
    {
        private Compiler compiler;
        private string name;
        private List<Parameter> parameters;
        private AbstractType returnType;
        private int parameterOffset;
        private int localVariableOffset;
        private int returnOffset;
        private Label entryLabel;
        private Label returnLabel;

        public Compiler Compiler
        {
            get
            {
                return compiler;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Parameter this[int index]
        {
            get
            {
                return parameters[index];
            }
        }

        public int ParamCount
        {
            get
            {
                return parameters.Count;
            }
        }

        public AbstractType ReturnType
        {
            get
            {
                return returnType;
            }

            set
            {
                returnType = value;

                returnOffset = parameterOffset;
                if (returnType != null)
                    returnOffset -= Compiler.GetSizeInDWords(returnType.Size());
            }
        }

        public int ParameterOffset
        {
            get
            {
                return parameterOffset;
            }
        }

        public int LocalVariableOffset
        {
            get
            {
                return localVariableOffset;
            }
        }

        public int ReturnOffset
        {
            get
            {
                return returnOffset;
            }
        }

        public Label EntryLabel
        {
            get
            {
                return entryLabel;
            }
        }

        public Label ReturnLabel
        {
            get
            {
                return returnLabel;
            }
        }


        public Function(Compiler compiler, string name)
        {
            this.compiler = compiler;
            this.name = name;

            parameters = new List<Parameter>();
            returnType = null;
            parameterOffset = -2;
            localVariableOffset = 0;
            returnOffset = -2;
        }

        internal void CreateEntryLabel()
        {
            entryLabel = compiler.CreateLabel();
        }

        internal void CreateReturnLabel()
        {
            returnLabel = compiler.CreateLabel();
        }

        internal void BindEntryLabel(Assembler assembler)
        {
            assembler.BindLabel(entryLabel);
        }

        internal void BindReturnLabel(Assembler assembler)
        {
            assembler.BindLabel(returnLabel);
        }

        internal void BeginBlock(Assembler assembler)
        {
            assembler.EmitLoadSP();
            assembler.EmitLoadConst(localVariableOffset);
            assembler.EmitAdd();
            assembler.EmitStoreSP();
        }

        internal void EndBlock(Assembler assembler)
        {            
            assembler.EmitLoadSP();
            assembler.EmitLoadConst(localVariableOffset);
            assembler.EmitSub();
            assembler.EmitStoreSP();
            assembler.EmitRet();
        }

        public Parameter FindParameter(string name)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                Parameter p = parameters[i];
                if (p.Name == name)
                    return p;
            }

            return null;
        }

        public Parameter DeclareParameter(string name, AbstractType type)
        {
            Parameter result = FindParameter(name);
            if (result != null)
                return null;

            parameterOffset -= Compiler.GetSizeInDWords(type.Size());
            result = new Parameter(this, name, type, parameterOffset);            
            parameters.Add(result);

            returnOffset = parameterOffset;
            if (returnType != null)
                returnOffset -= Compiler.GetSizeInDWords(returnType.Size());

            return result;
        }

        internal void CheckLocalVariableOffset(int offset)
        {
            if (offset > localVariableOffset)
                localVariableOffset = offset;
        }
    }
}
