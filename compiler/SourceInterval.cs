using System;

namespace compiler;

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
    public int LasttLine
    {
        get;
    }

    /// <summary>
    /// Comprimento do intervalo (em quantidade de caracteres).
    /// </summary>
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

    /// <summary>
    /// Verifica se o intervalo é válido.
    /// </summary>
    /// <returns>true se o intervalo for válido, false caso contrário.</returns>
    public bool IsValid()
    {
        return Start >= 0 && End >= 0 && FirstLine >= 1 && LasttLine >= 1 && Start <= End && FirstLine <= LasttLine;
    }

    /// <summary>
    /// Mescla dois intervalos.
    /// </summary>
    /// <param name="interval1">Intervalo 1</param>
    /// <param name="interval2">Intervalo 2</param>
    /// <returns>União dos dois intervalos.</returns>
    /// <exception cref="Exception">Se os dois intervalos são de fontes distintos.</exception>
    public static SourceInterval Merge(SourceInterval interval1, SourceInterval interval2)
    {
        if (interval1.FileName != interval2.FileName)
            throw new Exception("Não é possível mesclar intervalos de diferentes fontes.");

        int firstLine = Math.Min(interval1.FirstLine, interval2.FirstLine);
        int lastLine = Math.Max(interval1.LasttLine, interval2.LasttLine);
        int start = Math.Min(interval1.Start, interval2.Start);
        int end = Math.Max(interval1.End, interval2.End);
        return new SourceInterval(interval1.FileName, start, end, firstLine, lastLine);
    }

    /// <summary>
    /// Realiza a intersecção entre dois intervalos.
    /// </summary>
    /// <param name="interval1">Intervalo 1</param>
    /// <param name="interval2">Intervalo 2</param>
    /// <returns>Intersecção dos dois intervalos, sendo ela válida (não vazia) ou não.</returns>
    /// <exception cref="Exception">Se os dois intervalos são de fontes distintos.</exception>
    public static SourceInterval Intersect(SourceInterval interval1, SourceInterval interval2)
    {
        if (interval1.FileName != interval2.FileName)
            throw new Exception("Não é possível realizar a intersecção de intervalos de diferentes fontes.");

        int firstLine = Math.Max(interval1.FirstLine, interval2.FirstLine);
        int lastLine = Math.Min(interval1.LasttLine, interval2.LasttLine);
        int start = Math.Max(interval1.Start, interval2.Start);
        int end = Math.Min(interval1.End, interval2.End);
        return new SourceInterval(interval1.FileName, start, end, firstLine, lastLine);
    }

    /// <summary>
    /// Anexa um segundo intervalo ao final do primeiro inteervalo.
    /// </summary>
    /// <param name="interval1">Intervalo 1</param>
    /// <param name="interval2">Intervalo 2</param>
    /// <returns>Intervalo 1 estendido para a direita com os limites superiores do intervalo 2.</returns>
    /// <exception cref="Exception">Se os dois intervalos são de fontes distintos.</exception>
    public static SourceInterval Append(SourceInterval interval1, SourceInterval interval2)
    {
        if (interval1.FileName != interval2.FileName)
            throw new Exception("Não é possível anexar um intervalo de uma fonte e outro de outra fonte.");

        int firstLine = interval1.FirstLine;
        int lastLine = Math.Max(interval1.LasttLine, interval2.LasttLine);
        int start = interval1.Start;
        int end = Math.Max(interval1.End, interval2.End);
        return new SourceInterval(interval1.FileName, start, end, firstLine, lastLine);
    }

    /// <summary>
    /// Anexa um segundo intervalo no início do primeiro intervalo
    /// </summary>
    /// <param name="interval1">Intervalo 1</param>
    /// <param name="interval2">Intervalo 2</param>
    /// <returns>Intervalo 1 estendido para a esquerda com os limites inferiores do intervalo 2.</returns>
    /// <exception cref="Exception">Se os dois intervalos são de fontes distintos.</exception>
    public static SourceInterval Prepend(SourceInterval interval1, SourceInterval interval2)
    {
        if (interval1.FileName != interval2.FileName)
            throw new Exception("Não é possível anexar um intervalo de uma fonte e outro de outra fonte.");

        int firstLine = Math.Min(interval1.FirstLine, interval2.FirstLine);
        int lastLine = interval1.LasttLine;
        int start = Math.Min(interval1.Start, interval2.Start);
        int end = interval1.End;
        return new SourceInterval(interval1.FileName, start, end, firstLine, lastLine);
    }
}
