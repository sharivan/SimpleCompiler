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
        private CompilationUnity unity;
        private string name;
        private SourceInterval interval;
        private bool isExtern;
        private List<Parameter> parameters;
        private AbstractType returnType;

        private BlockStatement block;
        private int parameterOffset;
        private int parameterSize;
        private int localVariableOffset;
        private int returnOffset;
        private Label entryLabel;
        private Label returnLabel;

        public CompilationUnity Unity => unity;

        public string Name => name;

        public SourceInterval Interval => interval;

        public bool IsExtern => isExtern;

        public Parameter this[int index] => parameters[index];

        public int ParamCount => parameters.Count;

        public AbstractType ReturnType
        {
            get => returnType;

            internal set => returnType = value;
        }

        public BlockStatement Block
        {
            get => block;

            internal set => block = value;
        }

        public int ParameterOffset => parameterOffset;

        public int ParameterSize => parameterSize;

        public int LocalVariableOffset => localVariableOffset;

        public int ReturnOffset => returnOffset;

        public Label EntryLabel => entryLabel;

        public Label ReturnLabel => returnLabel;


        internal Function(CompilationUnity unity, string name, SourceInterval interval, bool isExtern = false)
        {
            this.unity = unity;
            this.name = name;
            this.interval = interval;
            this.isExtern = isExtern;

            parameters = new List<Parameter>();
            returnType = PrimitiveType.VOID;
            parameterOffset = -2 * sizeof(int);
            parameterSize = 0;
            localVariableOffset = 0;
            returnOffset = -2 * sizeof(int);
        }

        internal void CreateEntryLabel()
        {
            entryLabel = unity.Compiler.CreateLabel();
        }

        internal void CreateReturnLabel()
        {
            returnLabel = unity.Compiler.CreateLabel();
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

        internal Parameter DeclareParameter(string name, AbstractType type, SourceInterval interval, bool byRef)
        {
            Parameter result = FindParameter(name);
            if (result != null)
                return null;

            result = new Parameter(this, name, type, interval, parameterOffset, byRef);            
            parameters.Add(result);

            return result;
        }

        internal void Resolve()
        {
            parameterOffset = -2 * sizeof(int);
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                Parameter parameter = parameters[i];
                parameter.Resolve();               
                AbstractType paramType = parameter.Type;
                int size = parameter.ByRef ? IntPtr.Size : Compiler.GetAlignedSize(paramType.Size());
                parameterOffset -= size;
                parameter.Offset = parameterOffset;
            }

            returnOffset = parameterOffset;
            AbstractType.Resolve(ref returnType);
            if (!PrimitiveType.IsPrimitiveVoid(returnType))
                returnOffset -= Compiler.GetAlignedSize(returnType.Size());
        }

        internal void CheckLocalVariableOffset(int offset)
        {
            if (offset > localVariableOffset)
                localVariableOffset = offset;
        }
    }
}
