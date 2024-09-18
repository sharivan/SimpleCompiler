namespace Comp.Types;

internal class TypeEntry
{
    public int offset;
    public AbstractType type;
    public bool acquired;
    public bool tempVar;

    internal TypeEntry(int offset, AbstractType type, bool acquired, bool tempVar)
    {
        this.offset = offset;
        this.type = type;
        this.acquired = acquired;
        this.tempVar = tempVar;
    }
}