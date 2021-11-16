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
        private List<TypeSetType> typeSets;
        private Dictionary<string, TypeSetType> typeSetTable;
        private List<Function> functions;
        private Dictionary<string, Function> functionTable;
        private Dictionary<string, int> stringTable;
        private List<UnresolvedType> undeclaredTypes;
        private Dictionary<string, UnresolvedType> undeclaredTypeTable;

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

        public int StructCount => structs.Count;

        public int TypeSetCount => typeSets.Count;

        public int FunctionCount => functions.Count;

        public int StringTableCount => stringTable.Count;

        public int UndeclaredCount => undeclaredTypes.Count;

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

            imports = new();
            importTable = new();
            globals = new();
            globalTable = new();
            structs = new();
            structTable = new();
            typeSets = new();
            typeSetTable = new();
            functions = new();
            functionTable = new();           
            stringTable = new();
            undeclaredTypes = new();
            undeclaredTypeTable = new();

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

        public GlobalVariable GetGlobalVariable(int index)
        {
            return globals[index];
        }

        internal GlobalVariable DeclareGlobalVariable(string name, AbstractType type, SourceInterval interval)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(this, name, type, interval);            
            globals.Add(result);
            globalTable.Add(name, result);
            return result;
        }

        internal GlobalVariable DeclareGlobalVariable(string name, AbstractType type, SourceInterval interval, object initialValue)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(this, name, type, interval, globalVariableOffset, initialValue);
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

        public StructType GetStruct(int index)
        {
            return structs[index];
        }

        internal StructType DeclareStruct(string name, SourceInterval interval)
        {
            NamedType nt = FindNamedType(name);
            if (nt != null)
                return null;

            StructType result = new StructType(this, name, interval);
            structs.Add(result);
            structTable.Add(name, result);
            return result;
        }

        public TypeSetType FindTypeSet(string name, bool searchInImports = true)
        {
            if (typeSetTable.TryGetValue(name, out TypeSetType result))
                return result;

            if (searchInImports)
                foreach (CompilationUnity unity in imports)
                {
                    result = unity.FindTypeSet(name, false);
                    if (result != null)
                        return result;
                }

            return null;
        }

        public TypeSetType GetTypeSet(int index)
        {
            return typeSets[index];
        }

        internal TypeSetType DeclareTypeSet(string name, AbstractType type, SourceInterval interval)
        {
            NamedType nt = FindNamedType(name);
            if (nt != null)
                return null;

            TypeSetType result = new TypeSetType(this, name, type, interval);
            typeSets.Add(result);
            typeSetTable.Add(name, result);
            return result;
        }

        public NamedType FindNamedType(string name)
        {
            StructType st = FindStruct(name);
            if (st != null)
                return st;

            return FindTypeSet(name);
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

        internal Function DeclareFunction(string name, SourceInterval interval, bool isExtern)
        {
            Function result = FindFunction(name);
            if (result != null)
                return null;

            result = new Function(this, name, interval, isExtern);
            functions.Add(result);
            functionTable.Add(name, result);
            return result;
        }

        internal UnresolvedType FindUndeclaredType(string name)
        {
            if (undeclaredTypeTable.TryGetValue(name, out UnresolvedType result))
                return result;

            return null;
        }

        internal UnresolvedType AddUndeclaredType(string name, SourceInterval interval)
        {
            UnresolvedType result = FindUndeclaredType(name);
            if (result != null)
                return result;

            result = new UnresolvedType(this, name, interval);
            undeclaredTypes.Add(result);
            undeclaredTypeTable.Add(name, result);
            return result;
        }

        public UnresolvedType GetUndeclaredType(int index)
        {
            return undeclaredTypes[index];
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

        internal void Resolve()
        {
            foreach (UnresolvedType type in undeclaredTypes)
            {
                StructType st = FindStruct(type.Name);
                if (st == null)
                    throw new CompilerException(type.Interval, "Tipo não declarado '" + type.Name + "'.");

                type.ReferencedType = st;
            }

            foreach (StructType st in structs)
                st.Resolve();

            foreach (TypeSetType ts in typeSets)
                ts.Resolve();

            globalVariableOffset = 0;
            foreach (GlobalVariable global in globals)
            {
                global.Resolve();
                global.Offset = globalVariableOffset;
                globalVariableOffset += Compiler.GetAlignedSize(global.Type.Size());
            }

            foreach (Function function in functions)
                function.Resolve();

            undeclaredTypes.Clear();
            undeclaredTypeTable.Clear();
        }
    }
}
