namespace vm
{
    public class Breakpoint
    {
        internal Opcode opcode;
        internal bool enabled;
        internal bool temporary;

        public int IP
        {
            get;
        }

        public bool Temporary => temporary;

        public bool Enabled
        {
            get => enabled;

            set => enabled = value;
        }

        internal Breakpoint(int ip, Opcode opcode, bool temporary, bool enabled)
        {
            IP = ip;          
            this.opcode = opcode;           
            this.temporary = temporary;
            this.enabled = enabled;
        }

        public bool Equals(Breakpoint other) => other is not null && IP == other.IP;

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this))
                return true;

            if (other is null)
                return false;

            var otherBreakPoint = other as Breakpoint;
            return otherBreakPoint != null && IP == otherBreakPoint.IP;
        }

        public override int GetHashCode() => 243971260 + IP.GetHashCode();

        public static bool operator ==(Breakpoint left, Breakpoint right) => left is not null ? left.Equals(right) : right is null;

        public static bool operator !=(Breakpoint left, Breakpoint right) => !(left == right);
    }
}
