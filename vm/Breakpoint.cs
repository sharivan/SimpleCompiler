using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vm
{
    public class Breakpoint
    {
        private int ip;
        internal Opcode opcode;
        internal bool enabled;
        internal bool temporary;        

        public int IP => ip;

        public bool Temporary => temporary;

        public bool Enabled
        {
            get => enabled;

            set => enabled = value;
        }

        internal Breakpoint(int ip, Opcode opcode, bool temporary, bool enabled)
        {
            this.ip = ip;          
            this.opcode = opcode;           
            this.temporary = temporary;
            this.enabled = enabled;
        }

        public bool Equals(Breakpoint other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return ip == other.ip;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            Breakpoint otherBreakPoint = other as Breakpoint;
            if (otherBreakPoint == null)
                return false;

            return ip == otherBreakPoint.ip;
        }

        public override int GetHashCode()
        {
            return 243971260 + ip.GetHashCode();
        }

        public static bool operator ==(Breakpoint left, Breakpoint right)
        {
            return !ReferenceEquals(left, null) ? left.Equals(right) : ReferenceEquals(right, null);
        }

        public static bool operator !=(Breakpoint left, Breakpoint right)
        {
            return !(left == right);
        }
    }
}
