using compiler.types;

namespace compiler
{
    public abstract class Variable
    {
        private AbstractType type;

        public string Name
        {
            get;
        }

        public AbstractType Type => type;

        public SourceInterval Interval
        {
            get;
        }

        public int Offset { get; internal set; }

        protected Variable(string name, AbstractType type, SourceInterval interval, int offset = -1)
        {
            Name = name;
            this.type = type;
            Interval = interval;
            Offset = offset;
        }

        public override string ToString() => $"{Name}:{type} ({Offset})";

        internal void Resolve() => AbstractType.Resolve(ref type);
    }
}
