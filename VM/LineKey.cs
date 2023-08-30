using System;
using System.Collections.Generic;

namespace SimpleCompiler.VM;

public readonly struct LineKey(string fileName, int lineNumber) : IComparable<LineKey>, IEquatable<LineKey>
{
    public static readonly LineKey INVALID_KEY = new(null, -1);

    public string FileName
    {
        get;
    } = fileName;

    public int LineNumber
    {
        get;
    } = lineNumber;

    public int CompareTo(LineKey other)
    {
        int compare = FileName.CompareTo(other.FileName);
        return compare != 0 ? compare : LineNumber.CompareTo(other.LineNumber);
    }

    public bool Equals(LineKey other)
    {
        return FileName == other.FileName && LineNumber == other.LineNumber;
    }

    public override int GetHashCode()
    {
        int hashCode = 1924424622;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
        hashCode = hashCode * -1521134295 + LineNumber.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return $"{{FileName: \"{FileName}\" LineNumber: {LineNumber}}}";
    }

    public void Deconstruct(out string fileName, out int lineNumber)
    {
        fileName = FileName;
        lineNumber = LineNumber;
    }

    public static implicit operator LineKey((string fileName, int lineNumber) tuple)
    {
        return new LineKey(tuple.fileName, tuple.lineNumber);
    }

    public static implicit operator (string, int)(LineKey key)
    {
        return (key.FileName, key.LineNumber);
    }
}