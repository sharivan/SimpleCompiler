using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public class CompilerException : Exception
    {
        private SourceInterval interval;

        public SourceInterval Interval => interval;

        public CompilerException() : this(new SourceInterval(null, -1, -1, -1))
        {
        }

        public CompilerException(SourceInterval interval) : base()
        {
            this.interval = interval;
        }

        public CompilerException(string message) : this(new SourceInterval(null, -1, -1, -1), message)
        {
        }

        public CompilerException(SourceInterval interval, string message) : base(message)
        {
            this.interval = interval;
        }

        public CompilerException(string message, Exception cause) : this(new SourceInterval(null, -1, -1, -1), message, cause)
        {
        }

        public CompilerException(SourceInterval interval, string message, Exception cause) : base(message, cause)
        {
            this.interval = interval;
        }
    }
}
