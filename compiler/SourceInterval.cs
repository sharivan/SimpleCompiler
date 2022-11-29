using System;

namespace compiler
{
    public readonly struct SourceInterval
    {
        public string FileName
        {
            get;
        }

        public int Start
        {
            get;
        }

        public int End
        {
            get;
        }

        public int Line
        {
            get;
        }

        public int Length => End - Start;

        public SourceInterval(string fileName, int start, int end, int line)
        {
            if (end < start)
                end = start;

            FileName = fileName;
            Start = start;
            End = end;
            Line = line;
        }

        public bool IsValid() => Start >= 0;

        public static SourceInterval Merge(SourceInterval interval1, SourceInterval interval2)
        {
            if (interval1.FileName != interval2.FileName)
                throw new Exception("Can't merge source intervals from diferent sources.");

            int line = Math.Min(interval1.Line, interval2.Line);
            int start = Math.Min(interval1.Start, interval2.Start);
            int end = Math.Max(interval1.End, interval2.End);
            return new SourceInterval(interval1.FileName, start, end, line);
        }
    }
}
