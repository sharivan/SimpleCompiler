using System;
using System.Collections.Generic;

using compiler.types;
using assembler;

namespace compiler
{
    public class Function
    {
        private readonly List<Parameter> parameters;
        private AbstractType returnType;

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
        }

        public bool IsExtern
        {
            get;
        }

        public Parameter this[int index] => parameters[index];

        public int ParamCount => parameters.Count;

        public AbstractType ReturnType
        {
            get => returnType;

            internal set => returnType = value;
        }

        public BlockStatement Block { get;
            internal set; }

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

        internal Function(CompilationUnity unity, string name, SourceInterval interval, bool isExtern = false)
        {
            Unity = unity;
            Name = name;
            Interval = interval;
            IsExtern = isExtern;

            parameters = new List<Parameter>();
            returnType = PrimitiveType.VOID;
            ParameterOffset = -2 * sizeof(int);
            ParameterSize = 0;
            LocalVariableOffset = 0;
            ReturnOffset = -2 * sizeof(int);
        }

        internal void CreateEntryLabel() => EntryLabel = Unity.Compiler.CreateLabel();

        internal void CreateReturnLabel() => ReturnLabel = Unity.Compiler.CreateLabel();

        internal void BindEntryLabel(Assembler assembler) => assembler.BindLabel(EntryLabel);

        internal void BindReturnLabel(Assembler assembler) => assembler.BindLabel(ReturnLabel);

        internal void BeginBlock(Assembler assembler)
        {
            if (LocalVariableOffset > 0)
                assembler.EmitAddSP(LocalVariableOffset);
        }

        internal void EndBlock(Assembler assembler)
        {
            foreach (Parameter p in parameters)
            {
                AbstractType type = p.Type;
                if (type is StringType)
                {
                    Function f = Unity.Compiler.unitySystem.FindFunction("DecrementaReferenciaString");
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
            foreach (Parameter p in parameters)
                if (p.Name == name)
                    return p;

            return null;
        }

        internal Parameter DeclareParameter(string name, AbstractType type, SourceInterval interval, bool byRef)
        {
            Parameter result = FindParameter(name);
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
                Parameter parameter = parameters[i];
                parameter.Resolve();               
                AbstractType paramType = parameter.Type;
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

        internal void CheckLocalVariableOffset(int offset)
        {
            if (offset > LocalVariableOffset)
                LocalVariableOffset = offset;
        }
    }
}
