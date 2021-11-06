using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public struct SourceInterval
    {
        private int start;
        private int end;
        private int line;

        public int Start => start;

        public int End => end;

        public int Line => line;

        public SourceInterval(int start, int end, int line)
        {
            if (end < start)
                end = start;

            this.start = start;
            this.end = end;
            this.line = line;
        }

        public static SourceInterval Merge(SourceInterval interval1, SourceInterval interval2)
        {
            int line = Math.Min(interval1.line, interval2.line);
            int start = Math.Min(interval1.start, interval2.start);
            int end = Math.Max(interval1.end, interval2.end);
            return new SourceInterval(start, end, line);
        }
    }
}
