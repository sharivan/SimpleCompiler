using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public class ParserException : Exception
    {
        public ParserException() : base()
        {
        }

        public ParserException(string message) : base(message)
        {
        }

        public ParserException(Exception cause) : base("Parser Exception", cause)
        {
        }

        public ParserException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}
