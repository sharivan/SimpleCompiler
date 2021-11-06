using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public class CompilerException : Exception
    {
        private SourceInterval interval;

        public SourceInterval Interval => interval;

        public CompilerException(SourceInterval interval) : base()
        {
            this.interval = interval;
        }

        public CompilerException(SourceInterval interval, string message) : base(message)
        {
            this.interval = interval;
        }

        public CompilerException(SourceInterval interval, Exception cause) : base("Parser Exception", cause)
        {
            this.interval = interval;
        }

        public CompilerException(SourceInterval interval, string message, Exception cause) : base(message, cause)
        {
            this.interval = interval;
        }
    }
}
