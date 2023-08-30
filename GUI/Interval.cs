using System;

namespace SimpleCompiler.GUI;

internal readonly struct Interval(int start, int end)
{
    public int Start
    {
        get;
    } = start;

    public int End
    {
        get;
    } = end;

    public override int GetHashCode()
    {
        return (Start << 16) + End;
    }

    public override bool Equals(object o)
    {
        if (o == null)
            return false;

        if (ReferenceEquals(this, 0))
            return true;

        var interval = o as Interval?;
        return interval != null && this == interval;
    }

    public static bool operator ==(Interval left, Interval right)
    {
        return left.Start == right.Start && left.End == right.End;
    }

    public static bool operator !=(Interval left, Interval right)
    {
        return left.Start != right.Start || left.End != right.End;
    }

    public static bool operator <(Interval left, Interval right)
    {
        return left.Start > right.Start && left.End < right.End;
    }

    public static bool operator >(Interval left, Interval right)
    {
        return right.Start > left.Start && right.End < left.End;
    }

    public static bool operator <=(Interval left, Interval right)
    {
        return left.Start >= right.Start && left.End <= right.End;
    }

    public static bool operator >=(Interval left, Interval right)
    {
        return right.Start >= left.Start && right.End <= left.End;
    }

    public static Interval operator &(Interval left, Interval right)
    {
        int min = Math.Max(left.Start, right.Start);
        int max = Math.Min(left.End, right.End);

        return new Interval(min, max);
    }

    public static Interval operator |(Interval left, Interval right)
    {
        int min = Math.Min(left.Start, right.Start);
        int max = Math.Max(left.End, right.End);

        return new Interval(min, max);
    }
}