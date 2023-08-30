using System;
using System.Collections.Generic;

namespace Comp;

/// <summary>
/// Estrutura que representa um intervalo em um código fonte.
/// 
/// Um intervalo é definido por dois limites, o inferior e o superior.
/// O limite inferior é definido pelo campo Start enquanto o limite superior é definido pelo campo End.
/// Além disso, temos os campos FirstLine e LastLine que indicam a primeira e última linha, respectivamente.
/// 
/// Um intevalo pode ser válido ou não, dependendo dos valores de seus limites. O método IsValid() verifica tal validade.
/// Um intervalo com limites superiores e inferiores iguais é considerado válido, sendo esse um intervalo de comprimento zero.
/// </summary>
public readonly struct SourceInterval
{
    public static readonly SourceInterval INVALID = new(null, -1, -1, -1, -1);

    /// <summary>
    /// Arquivo de origem do intervalo.
    /// </summary>
    public string FileName
    {
        get;
    }

    /// <summary>
    /// Posição do caracter que inicia o intervalo.
    /// </summary>
    public int Start
    {
        get;
    }

    /// <summary>
    /// Posição do caracter que finaliza o intervalo.
    /// </summary>
    public int End
    {
        get;
    }

    /// <summary>
    /// Número da primeira linha do intervalo.
    /// </summary>
    public int FirstLine
    {
        get;
    }

    /// <summary>
    /// Número da última linha do intervalo.
    /// </summary>
    public int LastLine
    {
        get;
    }

    /// <summary>
    /// Comprimento do intervalo (em quantidade de caracteres).
    /// </summary>
    public int Length => End - Start + 1;

    public int LineCount => LastLine - FirstLine + 1;

    public SourceInterval(string fileName, int start, int end, int firstLine, int lastLine)
    {
        if (end < start)
            end = start;

        FileName = fileName;
        Start = start;
        End = end;
        FirstLine = firstLine;
        LastLine = lastLine;
    }

    /// <summary>
    /// Verifica se o intervalo é válido.
    /// </summary>
    /// <returns>true se o intervalo for válido, false caso contrário.</returns>
    public bool IsValid()
    {
        return Start >= 0 && End >= 0 && FirstLine >= 1 && LastLine >= 1 && Start <= End && FirstLine <= LastLine;
    }

    public SourceInterval Merge(SourceInterval other)
    {
        if (FileName != other.FileName)
            throw new Exception("Não é possível mesclar intervalos de diferentes fontes.");

        int firstLine = Math.Min(FirstLine, other.FirstLine);
        int lastLine = Math.Max(LastLine, other.LastLine);
        int start = Math.Min(Start, other.Start);
        int end = Math.Max(End, other.End);
        return new SourceInterval(FileName, start, end, firstLine, lastLine);
    }

    public SourceInterval Intersect(SourceInterval other)
    {
        if (FileName != other.FileName)
            throw new Exception("Não é possível realizar a intersecção de intervalos de diferentes fontes.");

        int firstLine = Math.Max(FirstLine, other.FirstLine);
        int lastLine = Math.Min(LastLine, other.LastLine);
        int start = Math.Max(Start, other.Start);
        int end = Math.Min(End, other.End);
        return new SourceInterval(FileName, start, end, firstLine, lastLine);
    }

    public SourceInterval Append(SourceInterval other)
    {
        if (FileName != other.FileName)
            throw new Exception("Não é possível anexar um intervalo de uma fonte e outro de outra fonte.");

        int firstLine = FirstLine;
        int lastLine = Math.Max(LastLine, other.LastLine);
        int start = Start;
        int end = Math.Max(End, other.End);
        return new SourceInterval(FileName, start, end, firstLine, lastLine);
    }

    public SourceInterval Prepend(SourceInterval other)
    {
        if (FileName != other.FileName)
            throw new Exception("Não é possível anexar um intervalo de uma fonte e outro de outra fonte.");

        int firstLine = Math.Min(FirstLine, other.FirstLine);
        int lastLine = LastLine;
        int start = Math.Min(Start, other.Start);
        int end = End;
        return new SourceInterval(FileName, start, end, firstLine, lastLine);
    }

    public bool HasIntersection(SourceInterval other)
    {
        int firstLine = Math.Max(FirstLine, other.FirstLine);
        int lastLine = Math.Min(LastLine, other.LastLine);
        int start = Math.Max(Start, other.Start);
        int end = Math.Min(End, other.End);
        return FileName == other.FileName &&
            start >= 0 &&
            end >= 0 &&
            start <= end &&
            firstLine >= 1 &&
            lastLine >= 1 &&
            firstLine <= lastLine;
    }

    public bool Contains(SourceInterval other)
    {
        return FileName == other.FileName &&
            Start <= other.Start &&
            other.End <= End &&
            FirstLine <= other.FirstLine &&
            other.LastLine <= LastLine;
    }

    public bool Equals(SourceInterval other)
    {
        return FileName == other.FileName &&
            Start == other.Start &&
            End == other.End &&
            FirstLine == other.FirstLine &&
            LastLine == other.LastLine;
    }

    public bool Contains(int pos)
    {
        return Start <= pos && pos <= End;
    }

    public bool Contains(string fileName, int pos)
    {
        return fileName == FileName && Start <= pos && pos <= End;
    }

    public bool ContainsLine(int line)
    {
        return FirstLine <= line && line <= LastLine;
    }

    public bool ContainsLine(string fileName, int line)
    {
        return fileName == FileName && FirstLine <= line && line <= LastLine;
    }

    public override bool Equals(object obj)
    {
        return obj is SourceInterval interval && Equals(interval);
    }

    public override int GetHashCode()
    {
        int hashCode = -1345008998;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
        hashCode = hashCode * -1521134295 + Start.GetHashCode();
        hashCode = hashCode * -1521134295 + End.GetHashCode();
        hashCode = hashCode * -1521134295 + FirstLine.GetHashCode();
        hashCode = hashCode * -1521134295 + LastLine.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return $"{{FileName: \"{FileName}\" Start: {Start} End: {End} FirstLine: {FirstLine} LastLine: {LastLine}}}";
    }
}