using compiler.types;

namespace compiler
{
    public class GlobalVariable : Variable
    {
        public CompilationUnity Unity
        {
            get;
        }

        public bool HasInitialValue
        {
            get;
        }

        public object InitialValue { get;
            internal set; }

        public bool InitialValueBool => (bool) InitialValue;

        public byte InitialValueByte => (byte) InitialValue;

        public char InitialValueChar => (char) InitialValue;

        public short InitialValueShort => (short) InitialValue;

        public int InitialValueInt => (int) InitialValue;

        public long InitialValueLong => (long) InitialValue;

        public float InitialValueFloat => (float) InitialValue;

        public double InitialValueDouble => (double) InitialValue;

        public string InitialValueString => (string) InitialValue;

        internal GlobalVariable(CompilationUnity unity, string name, AbstractType type, SourceInterval interval, int offset = -1, object initialValue = null) : 
            base(name, type, interval, offset)
        {
            Unity = unity;
            InitialValue = initialValue;

            HasInitialValue = true;
        }
    }
}
