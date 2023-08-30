namespace SimpleCompiler.GUI;

internal class Line
{
    internal int m_iNumber;
    internal int m_iStartPos;
    internal int m_iEndPos;

    internal Line(int number, int start, int end)
    {
        m_iNumber = number;
        m_iStartPos = start;
        m_iEndPos = end;
    }
}