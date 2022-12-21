using System.Collections.Generic;

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

        private readonly List<CompilationUnity> imports;
        private readonly Dictionary<string, CompilationUnity> importTable;
        private readonly List<GlobalVariable> globals;
        private readonly Dictionary<string, GlobalVariable> globalTable;
        private readonly List<StructType> structs;
        private readonly Dictionary<string, StructType> structTable;
        private readonly List<TypeSetType> typeSets;
        private readonly Dictionary<string, TypeSetType> typeSetTable;
        private readonly List<Function> functions;
        private readonly Dictionary<string, Function> functionTable;
        private readonly Dictionary<string, int> stringTable;
        private readonly List<UnresolvedType> undeclaredTypes;
        private readonly Dictionary<string, UnresolvedType> undeclaredTypeTable;
        internal bool parsed;

        public Compiler Compiler
        {
            get;
        }

        public string Name
        {
            get;
        }

        public string FileName
        {
            get;
        }

        public int GlobalStartOffset
        {
            get;
            private set;
        }

        public bool IsUnity
        {
            get;
        }

        public int ImportCount => imports.Count;

        public int GlobalVariableSize
        {
            get;
            private set;
        }

        public int StructCount => structs.Count;

        public int TypeSetCount => typeSets.Count;

        public int FunctionCount => functions.Count;

        public int StringTableCount => stringTable.Count;

        public int UndeclaredCount => undeclaredTypes.Count;

        public Function EntryPoint
        {
            get;
            internal set;
        }

        public SourceInterval Interval
        {
            get;
            internal set;
        }

        internal CompilationUnity(Compiler compiler, string name, string fileName, bool isUnity = false)
        {
            Compiler = compiler;
            Name = name;
            FileName = fileName;
            IsUnity = isUnity;

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

            GlobalVariableSize = 0;
            EntryPoint = null;

            parsed = false;
        }

        internal ImportResult AddImport(string unityName, out CompilationUnity result)
        {
            CompilationUnity unity = Compiler.OpenUnity(unityName);
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

            if (importTable.ContainsKey(unity.Name))
                return ImportResult.UNITY_ALREADY_IMPORTED;

            result = unity;
            imports.Add(unity);
            importTable.Add(unity.Name, unity);
            return ImportResult.OK;
        }

        public CompilationUnity GetImport(int index) => imports[index];

        public CompilationUnity GetImport(string name) => importTable.TryGetValue(name, out CompilationUnity result) ? result : null;

        public int GetStringOffset(string value)
        {
            if (stringTable.TryGetValue(value, out int result))
                return result;

            int size = (value.Length + 1) * sizeof(char);
            int offset = GlobalVariableSize;
            stringTable.Add(value, offset);
            GlobalVariableSize += Compiler.GetAlignedSize(size);
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

        public GlobalVariable GetGlobalVariable(int index) => globals[index];

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

            result = new GlobalVariable(this, name, type, interval, GlobalVariableSize, initialValue);
            GlobalVariableSize += Compiler.GetAlignedSize(type.Size);
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

        public StructType GetStruct(int index) => structs[index];

        internal StructType DeclareStruct(string name, SourceInterval interval)
        {
            NamedType nt = FindNamedType(name);
            if (nt != null)
                return null;

            StructType result = new(this, name, interval);
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

        public TypeSetType GetTypeSet(int index) => typeSets[index];

        internal TypeSetType DeclareTypeSet(string name, AbstractType type, SourceInterval interval)
        {
            NamedType nt = FindNamedType(name);
            if (nt != null)
                return null;

            TypeSetType result = new(this, name, type, interval);
            typeSets.Add(result);
            typeSetTable.Add(name, result);
            return result;
        }

        public NamedType FindNamedType(string name)
        {
            StructType st = FindStruct(name);
            return st != null ? st : FindTypeSet(name);
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

        internal UnresolvedType FindUndeclaredType(string name) => undeclaredTypeTable.TryGetValue(name, out UnresolvedType result) ? result : null;

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

        public UnresolvedType GetUndeclaredType(int index) => undeclaredTypes[index];

        internal void Compile(Assembler assembler)
        {
            Compiler.unity = this;

            GlobalStartOffset = Compiler.globalVariableOffset;

            foreach (Function f in functions)
            {
                Compiler.function = f;
                Compiler.CompileFunction(assembler);
            }

            Compiler.globalVariableOffset += GlobalVariableSize;
        }

        internal void WriteConstants(Assembler assembler)
        {
            foreach (var kv in stringTable)
                assembler.WriteConstant(GlobalStartOffset + kv.Value, kv.Key);
        }

        internal void Resolve()
        {
            foreach (UnresolvedType type in undeclaredTypes)
            {
                StructType st = FindStruct(type.Name);

                type.ReferencedType = st ?? throw new CompilerException(type.Interval, $"Tipo não declarado: '{type.Name}'.");
            }

            foreach (StructType st in structs)
                st.Resolve();

            foreach (TypeSetType ts in typeSets)
                ts.Resolve();

            GlobalVariableSize = 0;
            foreach (GlobalVariable global in globals)
            {
                global.Resolve();
                global.Offset = GlobalVariableSize;
                GlobalVariableSize += Compiler.GetAlignedSize(global.Type.Size);
            }

            foreach (Function function in functions)
                function.Resolve();

            undeclaredTypes.Clear();
            undeclaredTypeTable.Clear();
        }

        internal void EmitStringRelease(Assembler assembler)
        {
            Context context = new(this, Interval);
            for (int i = 0; i < globals.Count; i++)
            {
                GlobalVariable g = globals[i];
                AbstractType type = g.Type;
                type.EmitStringRelease(context, Compiler, assembler, GlobalStartOffset + g.Offset, AbstractType.ReleaseType.GLOBAL);
            }
        }
    }
}
