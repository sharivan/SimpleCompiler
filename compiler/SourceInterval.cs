using System;

namespace compiler
{
    public readonly struct SourceInterval
    {
        public static readonly SourceInterval INVALID = new(null, -1, -1, -1, -1);

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

        public int FirstLine
        {
            get;
        }

        public int LasttLine
        {
            get;
        }

        public int Length => End - Start;

        public SourceInterval(string fileName, int start, int end, int firstLine, int lastLine)
        {
            if (end < start)
                end = start;

            FileName = fileName;
            Start = start;
            End = end;
            FirstLine = firstLine;
            LasttLine = lastLine;
        }

        public bool IsValid() => Start >= 0;

        public static SourceInterval Merge(SourceInterval interval1, SourceInterval interval2)
        {
            if (interval1.FileName != interval2.FileName)
                throw new Exception("Can't merge source intervals from diferent sources.");

            int firstLine = Math.Min(interval1.FirstLine, interval2.FirstLine);
            int lastLine = Math.Max(interval1.LasttLine, interval2.LasttLine);
            int start = Math.Min(interval1.Start, interval2.Start);
            int end = Math.Max(interval1.End, interval2.End);
            return new SourceInterval(interval1.FileName, start, end, firstLine, lastLine);
        }
    }
}
