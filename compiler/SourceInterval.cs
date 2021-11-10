using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public struct SourceInterval
    {
        private string fileName;
        private int start;
        private int end;
        private int line;

        public string FileName => fileName;

        public int Start => start;

        public int End => end;

        public int Line => line;

        public int Length => end - start;

        public SourceInterval(string fileName, int start, int end, int line)
        {
            if (end < start)
                end = start;

            this.fileName = fileName;
            this.start = start;
            this.end = end;
            this.line = line;
        }

        public bool IsValid()
        {
            return start >= 0;
        }

        public static SourceInterval Merge(SourceInterval interval1, SourceInterval interval2)
        {
            if (interval1.fileName != interval2.fileName)
                throw new Exception("Can't merge source intervals from diferent sources.");

            int line = Math.Min(interval1.line, interval2.line);
            int start = Math.Min(interval1.start, interval2.start);
            int end = Math.Max(interval1.end, interval2.end);
            return new SourceInterval(interval1.fileName, start, end, line);
        }
    }
}
