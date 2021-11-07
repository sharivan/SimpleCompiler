using System;
using System.Collections.Generic;
using System.Text;

namespace compiler.lexer
{
    public abstract class Token
    {
        private SourceInterval interval;

        public SourceInterval Interval => interval;

        protected Token(SourceInterval interval)
        {
            this.interval = interval;
        }
    }
}
