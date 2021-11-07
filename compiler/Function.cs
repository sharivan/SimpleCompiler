using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;
using assembler;

namespace compiler
{
    public class Function
    {
        private Compiler compiler;
        private string name;
        private List<Parameter> parameters;
        private AbstractType returnType;

        private BlockStatement block;
        private int parameterOffset;
        private int localVariableOffset;
        private int returnOffset;
        private Label entryLabel;
        private Label returnLabel;

        public Compiler Compiler => compiler;

        public string Name => name;

        public Parameter this[int index] => parameters[index];

        public int ParamCount => parameters.Count;

        public AbstractType ReturnType
        {
            get => returnType;

            set
            {
                returnType = value;

                returnOffset = parameterOffset;
                if (!PrimitiveType.IsPrimitiveVoid(returnType))
                    returnOffset -= returnType.Size();
            }
        }

        public BlockStatement Block
        {
            get => block;

            set => block = value;
        }

        public int ParameterOffset => parameterOffset;

        public int LocalVariableOffset => localVariableOffset;

        public int ReturnOffset => returnOffset;

        public Label EntryLabel => entryLabel;

        public Label ReturnLabel => returnLabel;


        public Function(Compiler compiler, string name)
        {
            this.compiler = compiler;
            this.name = name;

            parameters = new List<Parameter>();
            returnType = PrimitiveType.VOID;
            parameterOffset = -2 * sizeof(int);
            localVariableOffset = 0;
            returnOffset = -2 * sizeof(int);
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
            if (localVariableOffset > 0)
                assembler.EmitAddSP(localVariableOffset);
        }

        internal void EndBlock(Assembler assembler)
        {
            if (localVariableOffset > 0)
                assembler.EmitSubSP(localVariableOffset);

            int count = -parameterOffset - 2 * sizeof(int);
            if (count > 0)
                assembler.EmitRetN(count);
            else
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

        public Parameter DeclareParameter(string name, AbstractType type, bool byRef)
        {
            Parameter result = FindParameter(name);
            if (result != null)
                return null;

            parameterOffset -= byRef ? IntPtr.Size : type.Size();
            result = new Parameter(this, name, type, parameterOffset, byRef);            
            parameters.Add(result);

            returnOffset = parameterOffset;
            if (!PrimitiveType.IsPrimitiveVoid(returnType))
                returnOffset -= returnType.Size();

            return result;
        }

        public void ComputeParametersOffsets()
        {
            int offset = -2 * sizeof(int);
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                Parameter parameter = parameters[i];
                offset -= parameter.ByRef ? IntPtr.Size : parameter.Type.Size();
                parameter.Offset = offset;
            }
        }

        internal void CheckLocalVariableOffset(int offset)
        {
            if (offset > localVariableOffset)
                localVariableOffset = offset;
        }
    }
}
