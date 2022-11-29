using compiler.types;

namespace compiler
{
    public class Parameter : Variable
    {
        public Function Function
        {
            get;
        }

        public bool ByRef
        {
            get;
        }

        internal Parameter(Function function, string name, AbstractType type, SourceInterval interval, int offset = -1, bool byRef = false) : base(name, type, interval, offset)
        {
            Function = function;
            ByRef = byRef;
        }

        public override string ToString() => (ByRef ? "&" : "") + base.ToString();
    }
}
