using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;
using assembler;

namespace compiler
{
    public class CompilationUnity
    {
        public enum ImportResult
        {
            OK,
            UNITY_NOT_FOUND,
            UNITY_ALREADY_IMPORTED,
            UNITY_IS_PROGRAM,
            SELF_REFERENCE_UNITY 
        }

        private Compiler compiler;
        private string name;
        private string fileName;
        private int globalStartOffset;
        private bool isUnity;      

        private List<CompilationUnity> imports;
        private Dictionary<string, CompilationUnity> importTable;
        private List<GlobalVariable> globals;
        private Dictionary<string, GlobalVariable> globalTable;
        private List<StructType> structs;
        private Dictionary<string, StructType> structTable;
        private List<Function> functions;
        private Dictionary<string, Function> functionTable;
        private Dictionary<string, int> stringTable;

        private int globalVariableOffset;
        private Function entryPoint;

        internal bool parsed;

        public Compiler Compiler => compiler;

        public string Name => name;

        public string FileName => fileName;

        public int GlobalStartOffset => globalStartOffset;

        public bool IsUnity => isUnity;

        public int ImportCount => imports.Count;

        public int GlobalVariableSize => globalVariableOffset;

        public Function EntryPoint
        {
            get => entryPoint;

            internal set => entryPoint = value;
        }

        internal CompilationUnity(Compiler compiler, string name, string fileName, bool isUnity = false)
        {
            this.compiler = compiler;
            this.name = name;
            this.fileName = fileName;
            this.isUnity = isUnity;

            imports = new List<CompilationUnity>();
            importTable = new Dictionary<string, CompilationUnity>();
            globals = new List<GlobalVariable>();
            globalTable = new Dictionary<string, GlobalVariable>();
            structs = new List<StructType>();
            structTable = new Dictionary<string, StructType>();
            functions = new List<Function>();
            functionTable = new Dictionary<string, Function>();           
            stringTable = new Dictionary<string, int>();

            globalVariableOffset = 0;
            entryPoint = null;

            parsed = false;
        }

        internal ImportResult AddImport(string unityName, out CompilationUnity result)
        {
            CompilationUnity unity = compiler.OpenUnity(unityName);
            if (unity == null)
            {
                result = null;
                return ImportResult.UNITY_NOT_FOUND;
            }

            return AddImport(unity, out result);
        }

        internal ImportResult AddImport(CompilationUnity unity, out CompilationUnity result)
        {
            result = null;

            if (!unity.IsUnity)
                return ImportResult.UNITY_IS_PROGRAM;

            if (unity == this)
                return ImportResult.SELF_REFERENCE_UNITY;

            if (importTable.ContainsKey(unity.name))
                return ImportResult.UNITY_ALREADY_IMPORTED;

            result = unity;
            imports.Add(unity);
            importTable.Add(unity.name, unity);
            return ImportResult.OK;
        }

        public CompilationUnity GetImport(int index)
        {
            return imports[index];
        }

        public CompilationUnity GetImport(string name)
        {
            if (importTable.TryGetValue(name, out CompilationUnity result))
                return result;

            return null;
        }

        public int GetStringOffset(string value)
        {
            if (stringTable.TryGetValue(value, out int result))
                return result;

            int size = (value.Length + 1) * sizeof(char);
            int offset = globalVariableOffset;
            stringTable.Add(value, offset);
            globalVariableOffset += Compiler.GetAlignedSize(size);
            return offset;
        }

        public GlobalVariable FindGlobalVariable(string name, bool searchInImports = true)
        {
            if (globalTable.TryGetValue(name, out GlobalVariable result))
                return result;

            if (searchInImports)
                foreach (CompilationUnity unity in imports)
                {
                    result = unity.FindGlobalVariable(name, false);
                    if (result != null)
                        return result;
                }

            return null;
        }

        internal GlobalVariable DeclareGlobalVariable(string name, AbstractType type)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(this, name, type, globalVariableOffset);
            globalVariableOffset += Compiler.GetAlignedSize(type.Size());
            globals.Add(result);
            globalTable.Add(name, result);
            return result;
        }

        internal GlobalVariable DeclareGlobalVariable(string name, AbstractType type, object initialValue)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(this, name, type, globalVariableOffset, initialValue);
            globalVariableOffset += Compiler.GetAlignedSize(type.Size());
            globals.Add(result);
            globalTable.Add(name, result);
            return result;
        }

        public StructType FindStruct(string name, bool searchInImports = true)
        {
            if (structTable.TryGetValue(name, out StructType result))
                return result;

            if (searchInImports)
                foreach (CompilationUnity unity in imports)
                {
                    result = unity.FindStruct(name, false);
                    if (result != null)
                        return result;
                }

            return null;
        }

        internal StructType DeclareStruct(string name)
        {
            StructType result = FindStruct(name);
            if (result != null)
                return null;

            result = new StructType(this, name);
            structs.Add(result);
            structTable.Add(name, result);
            return result;
        }

        public Function FindFunction(string name, bool searchInImports = true)
        {
            if (functionTable.TryGetValue(name, out Function result))
                return result;

            if (searchInImports)
                foreach (CompilationUnity unity in imports)
                {
                    result = unity.FindFunction(name, false);
                    if (result != null)
                        return result;
                }

            return null;
        }

        internal Function DeclareFunction(string name, bool isExtern)
        {
            Function result = FindFunction(name);
            if (result != null)
                return null;

            result = new Function(this, name, isExtern);
            functions.Add(result);
            functionTable.Add(name, result);
            return result;
        }

        internal void Compile(Assembler assembler)
        {
            compiler.unity = this;

            globalStartOffset = compiler.globalVariableOffset;

            foreach (Function f in functions)
            {
                compiler.function = f;
                compiler.CompileFunction(assembler);
            }

            compiler.globalVariableOffset += globalVariableOffset;
        }

        internal void WriteConstants(Assembler assembler)
        {
            foreach (var kv in stringTable)
                assembler.WriteConstant(globalStartOffset + kv.Value, kv.Key);
        }
    }
}
