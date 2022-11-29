using compiler.types;

namespace compiler
{
    public class LocalVariable : Variable
    {
        public bool Temporary
        {
            get; internal set;
        }

        public bool Acquired
        {
            get; internal set;
        }

        public Function Function
        {
            get;
        }

        public bool Param => Offset < 0;

        internal LocalVariable(Function function, string name, AbstractType type, SourceInterval interval, int offset = -1) : base(name, type, interval, offset) => Function = function;

        internal void Release() => Acquired = false;
    }
}
