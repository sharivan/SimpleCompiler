using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;

namespace compiler
{
    public class GlobalVariable : Variable
    {
        private CompilationUnity unity;
        private bool initialValueSet;
        private object initialValue;

        public CompilationUnity Unity => unity;

        public bool HasInitialValue => initialValueSet;

        public object InitialValue
        {
            get => initialValue;

            internal set => initialValue = value;
        }

        public bool InitialValueBool => (bool) initialValue;

        public byte InitialValueByte => (byte) initialValue;

        public char InitialValueChar => (char) initialValue;

        public short InitialValueShort => (short) initialValue;

        public int InitialValueInt => (int) initialValue;

        public long InitialValueLong => (long) initialValue;

        public float InitialValueFloat => (float) initialValue;

        public double InitialValueDouble => (double) initialValue;

        public string InitialValueString => (string) initialValue;

        internal GlobalVariable(CompilationUnity unity, string name, AbstractType type, SourceInterval interval, int offset = -1, object initialValue = null) : 
            base(name, type, interval, offset)
        {
            this.unity = unity;
            this.initialValue = initialValue;

            initialValueSet = true;
        }
    }
}
