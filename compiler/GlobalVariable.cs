using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class GlobalVariable : Variable
    {
        private bool initialValueSet;
        private object initialValue;

        public bool HasInitialValue => initialValueSet;

        public object InitialValue
        {
            get => initialValue;
            set => initialValue = value;
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


        public GlobalVariable(string name, AbstractType type, int offset) :
            base(name, type, offset)
        {
            initialValueSet = false;
        }

        public GlobalVariable(string name, AbstractType type, int offset, object initialValue) : 
            base(name, type, offset)
        {
            this.initialValue = initialValue;

            initialValueSet = true;
        }
    }
}
