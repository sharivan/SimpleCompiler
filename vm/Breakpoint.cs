namespace VM;

public class Breakpoint
{
    internal string fileName;
    internal int line;
    internal Opcode opcode;
    internal bool enabled;
    internal bool temporary;

    public int IP
    {
        get;
    }

    public string FileName => fileName;

    public int Line => line;

    public bool Temporary => temporary;

    public bool Enabled
    {
        get => enabled;
        set => enabled = value;
    }

    internal Breakpoint(int ip, string fileName, int line, Opcode opcode, bool temporary, bool enabled)
    {
        IP = ip;
        this.fileName = fileName;
        this.line = line;
        this.opcode = opcode;
        this.temporary = temporary;
        this.enabled = enabled;
    }

    public bool Equals(Breakpoint other)
    {
        return other is not null && IP == other.IP;
    }

    public override bool Equals(object other)
    {
        if (ReferenceEquals(other, this))
            return true;

        if (other is null)
            return false;

        var otherBreakPoint = other as Breakpoint;
        return otherBreakPoint != null && IP == otherBreakPoint.IP;
    }

    public override int GetHashCode()
    {
        return 243971260 + IP.GetHashCode();
    }

    public static bool operator ==(Breakpoint left, Breakpoint right)
    {
        return left is not null ? left.Equals(right) : right is null;
    }

    public static bool operator !=(Breakpoint left, Breakpoint right)
    {
        return !(left == right);
    }
}
