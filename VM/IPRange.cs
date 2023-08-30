using System;

namespace SimpleCompiler.VM;

public readonly struct IPRange(int minIP, int maxIP) : IEquatable<IPRange>
{
    public int MinIP
    {
        get;
    } = minIP;

    public int MaxIP
    {
        get;
    } = maxIP;

    public bool Contains(int ip)
    {
        return ip >= MinIP && ip <= MaxIP;
    }

    public override string ToString()
    {
        return $"[{MinIP}, {MaxIP}]";
    }

    public void Deconstruct(out int minIP, out int maxIP)
    {
        minIP = MinIP;
        maxIP = MaxIP;
    }

    public bool Equals(IPRange other)
    {
        return MinIP == other.MinIP && MaxIP == other.MaxIP;
    }

    public static implicit operator IPRange((int minIP, int maxIP) tuple)
    {
        return new IPRange(tuple.minIP, tuple.maxIP);
    }

    public static implicit operator (int minIP, int maxIP)(IPRange range)
    {
        return (range.MinIP, range.MaxIP);
    }
}