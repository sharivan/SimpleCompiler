using System;

namespace compiler
{
    public class CompilerException : Exception
    {
        public SourceInterval Interval
        {
            get;
        }

        public CompilerException() : this(new SourceInterval(null, -1, -1, -1))
        {
        }

        public CompilerException(SourceInterval interval) : base() => Interval = interval;

        public CompilerException(string message) : this(new SourceInterval(null, -1, -1, -1), message)
        {
        }

        public CompilerException(SourceInterval interval, string message) : base(message) => Interval = interval;

        public CompilerException(string message, Exception cause) : this(new SourceInterval(null, -1, -1, -1), message, cause)
        {
        }

        public CompilerException(SourceInterval interval, string message, Exception cause) : base(message, cause) => Interval = interval;
    }
}
