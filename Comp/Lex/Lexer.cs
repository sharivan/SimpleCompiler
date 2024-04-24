using System;
using System.IO;

namespace Comp.Lex;

public class Lexer : IDisposable
{
    private class LocalBuffer(TextReader input) : IDisposable
    {
        private const int BUFFER_SIZE = 16;

        private readonly TextReader input = input;

        private readonly char[] buffer = new char[BUFFER_SIZE];
        private int pos = -1;
        private int len = 0;

        public char NextChar()
        {
            if (pos < len - 1)
                return buffer[++pos];

            int current = input.Read();
            char c = current == -1 ? '\0' : (char) current;

            if (pos == buffer.Length - 1)
            {
                int len = buffer.Length - 1;
                Array.Copy(buffer, 1, buffer, 0, len);
                buffer[len] = c;
            }
            else
            {
                buffer[++pos] = c;
                len++;
            }

            return c;
        }

        public int Undo(int count = 1)
        {
            if (pos - count < -1)
                throw new Exception($"Invalid undo position: {pos - count}");

            pos -= count;
            int lines = 0;
            for (int i = pos + 1; i < pos + count + 1; i++)
            {
                if (buffer[i] == '\n')
                    lines++;
            }

            return lines;
        }

        public void Dispose()
        {
            input.Close();
        }
    }

    public static Lexer CreateFromSource(string source)
    {
        return new(null, new StringReader(source));
    }

    public static Lexer CreateFromFile(string fileName)
    {
        return new(fileName, File.OpenText(fileName));
    }

    public static Lexer CreateFromReader(TextReader reader)
    {
        return new(null, reader);
    }

    public static Lexer CreateFromReader(string fileName, TextReader reader)
    {
        return new(fileName, reader);
    }

    private const int MAX_CACHED_TOKENS = 16;
    private readonly LocalBuffer input;
    private bool disposed;

    private readonly Token[] tokens;
    private int tokenIndex;
    private int tokenCount;

    public string FileName
    {
        get;
    }

    public int CurrentPos
    {
        get;
        private set;
    }

    public int CurrentLine
    {
        get;
        private set;
    }

    public SourceInterval CurrentInterval(int startPos, int startLine)
    {
        return new(FileName, startPos, CurrentPos, startLine, CurrentLine);
    }

    public SourceInterval CurrentInterval()
    {
        return CurrentInterval(CurrentPos, CurrentLine);
    }

    private Lexer(string fileName, TextReader input)
    {
        FileName = fileName;
        this.input = new LocalBuffer(input);

        tokens = new Token[MAX_CACHED_TOKENS];

        CurrentPos = 0;
        CurrentLine = 1;
        disposed = false;
        tokenIndex = -1;
        tokenCount = 0;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            input.Dispose();
            disposed = true;
        }
    }

    public Token CurrentToken()
    {
        return tokenIndex == -1 ? null : tokens[tokenIndex];
    }

    private char NextChar()
    {
        char result = input.NextChar();
        if (result == '\0')
            return '\0';

        CurrentPos++;

        if (result == '\n')
            CurrentLine++;

        return result;
    }

    private void Undo(int count = 1)
    {
        int lines = input.Undo(count);
        CurrentPos -= count;
        CurrentLine -= lines;
    }

    private void SkipBlanks()
    {
        char c = NextChar();
        if (c == '\0')
            return;

        while (c is ' ' or '\t' or '\n' or '\r')
            c = NextChar();

        Undo();
    }

    private void SkipComments()
    {
        int startPos = CurrentPos;
        int startLine = CurrentLine;
        char c = NextChar();
        if (c == '\0')
            return;

        if (c == '/')
        {
            c = NextChar();
            switch (c)
            {
                case '/':
                    while (true)
                    {
                        c = NextChar();
                        switch (c)
                        {
                            case '\0':
                                return;

                            case '\n':
                                return;
                        }
                    }

                case '*':
                    while (true)
                    {
                        c = NextChar();
                        switch (c)
                        {
                            case '\0':
                                throw new CompilerException(CurrentInterval(startPos, startLine), "Fim do comentário esperado mas fim do arquivo encontrado.");

                            case '*':
                                c = NextChar();
                                switch (c)
                                {
                                    case '\0':
                                        Undo();
                                        throw new CompilerException(CurrentInterval(startPos, startLine), "Fim do comentário esperado mas fim do arquivo encontrado.");

                                    case '/':
                                        return;
                                }

                                break;
                        }
                    }

                default:
                    Undo(2);
                    break;
            }
        }
        else
        {
            Undo();
        }
    }

    private char ParseChar(int startPos, int startLine, bool fromString)
    {
        int charStartPos = CurrentPos;
        int charStartLine = CurrentLine;
        char c = NextChar();
        switch (c)
        {
            case '\0': // fim de arquivo
                throw new CompilerException(CurrentInterval(startPos, startLine), "Caractere esperado mas fim do arquivo encontrado.");

            case '\n' or '\r': // quebra de linha ou retorno de carro
                Undo();
                throw new CompilerException(CurrentInterval(startPos, startLine), $"Delimitador de {(fromString ? "string" : "caractere")} esperado mas quebra de linha encontrada.");

            case '\\': // caractere escape
            {
                c = NextChar();
                switch (c)
                {
                    case '\0':
                        throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código de escape esperado mas fim do arquivo encontrado.");

                    case '\n' or '\r':
                        Undo();
                        throw new CompilerException(CurrentInterval(charStartPos, charStartLine), $"Delimitador de {(fromString ? "string" : "caractere")} esperado mas quebra de linha encontrada.");

                    case 'n': // quebra de linha
                        return '\n';

                    case 'r': // retorno de carro
                        return '\r';

                    case 't': // tabulação
                        return '\t';

                    case 'u': // hexadecimal unicode
                    {
                        string str = "";
                        while (true)
                        {
                            c = NextChar();
                            if (c == '\0')
                                throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código unicode esperado mas fim do arquivo encontrado.");

                            if (c is '\n' or '\r')
                            {
                                Undo();
                                throw new CompilerException(CurrentInterval(charStartPos, charStartLine), $"Delimitador de {(fromString ? "string" : "caractere")} esperado mas quebra de linha encontrada.");
                            }

                            if (c is not (>= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f'))
                            {
                                Undo();
                                break;
                            }
                        }

                        if (str == "")
                            throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código unicode esperado.");

                        int hex;
                        try
                        {
                            hex = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        }
                        catch (FormatException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Formato de código unicode inválido.");
                        }
                        catch (OverflowException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Estouro na conversão de código unicode.");
                        }

                        return hex is < 0 or >= short.MaxValue
                            ? throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código unicode fora da faixa.")
                            : (char) hex;
                    }

                    case '0': // octal
                    {
                        string str = "";
                        while (true)
                        {
                            c = NextChar();
                            if (c == '\0')
                                throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código octal esperado mas fim do arquivo encontrado.");

                            if (c is '\n' or '\r')
                            {
                                Undo();
                                throw new CompilerException(CurrentInterval(charStartPos, charStartLine), $"Delimitador de {(fromString ? "string" : "caractere")} esperado mas quebra de linha encontrada.");
                            }

                            if (c is not (>= '0' and <= '7'))
                            {
                                Undo();
                                break;
                            }
                        }

                        if (str == "")
                            throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código octal esperado.");

                        int oct;
                        try
                        {
                            oct = Convert.ToInt32(str, 8);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código octal fora da faixa.");
                        }
                        catch (FormatException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Formato de código octal inválido.");
                        }
                        catch (OverflowException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Estouro na conversão de código octal.");
                        }

                        return oct is < 0 or >= short.MaxValue
                            ? throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código octal fora da faixa.")
                            : (char) oct;
                    }

                    case '\\':
                        return '\\';

                    case '\'':
                        return '\'';

                    case '"':
                        return '"';
                }

                Undo();
                throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Código de escape inválido.");
            }

            case '\'':
                if (!fromString)
                {
                    Undo();
                    throw new CompilerException(CurrentInterval(charStartPos, charStartLine), "Caractere esperado.");
                }

                return '\'';
        }

        return c;
    }

    private ByteLiteral ParseByte(int startPos, int startLine, string number)
    {
        try
        {
            byte value = byte.Parse(number);
            return new ByteLiteral(CurrentInterval(startPos, startLine), value);
        }
        catch (FormatException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Formato de literal byte inválido: {number}", e);
        }
        catch (OverflowException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Overflow durante a conversão do literal byte: {number}", e);
        }
    }

    private ShortLiteral ParseShort(int startPos, int startLine, string number)
    {
        try
        {
            short value = short.Parse(number);
            return new ShortLiteral(CurrentInterval(startPos, startLine), value);
        }
        catch (FormatException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Formato de literal inteiro curto inválido: {number}", e);
        }
        catch (OverflowException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Overflow durante a conversão do literal inteiro curto: {number}", e);
        }
    }

    private IntLiteral ParseInt(int startPos, int startLine, string number)
    {
        try
        {
            int value = int.Parse(number);
            return new IntLiteral(CurrentInterval(startPos, startLine), value);
        }
        catch (FormatException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Formato de literal inteiro inválido: {number}", e);
        }
        catch (OverflowException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Overflow durante a conversão do literal inteiro: {number}", e);
        }
    }

    private LongLiteral ParseLong(int startPos, int startLine, string number)
    {
        try
        {
            long value = long.Parse(number);
            return new LongLiteral(CurrentInterval(startPos, startLine), value);
        }
        catch (FormatException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Formato de literal inteiro longo inválido: {number}", e);
        }
        catch (OverflowException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Overflow durante a conversão do literal inteiro longo: {number}", e);
        }
    }

    private FloatLiteral ParseFloat(int startPos, int startLine, string number)
    {
        try
        {
            float value = float.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
            return new FloatLiteral(CurrentInterval(startPos, startLine), value);
        }
        catch (FormatException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Formato de literal de ponto flutuante inválido: {number}", e);
        }
        catch (OverflowException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Overflow durante a conversão do literal de ponto flutuante: {number}", e);
        }
    }

    private DoubleLiteral ParseDouble(int startPos, int startLine, string number)
    {
        try
        {
            double value = double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
            return new DoubleLiteral(CurrentInterval(startPos, startLine), value);
        }
        catch (FormatException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Formato de literal de ponto flutuante inválido: {number}", e);
        }
        catch (OverflowException e)
        {
            throw new CompilerException(CurrentInterval(startPos, startLine), $"Overflow durante a conversão do literal de ponto flutuante: {number}", e);
        }
    }

    public Token NextToken()
    {
        if (tokenIndex < tokenCount - 1)
            return tokens[++tokenIndex];

        int lastPos;
        while (true)
        {
            lastPos = CurrentPos;

            SkipBlanks();
            SkipComments();

            if (lastPos == CurrentPos)
                break;
        }

        lastPos = CurrentPos;
        int lastLine = CurrentLine;
        char c = NextChar();

        Token result = null;

        if (c != '\0')
        {
            switch (c)
            {
                case >= '0' and <= '9': // dígito numérico (pode ser um número)
                {
                    string number = c + "";
                    c = NextChar();
                    while (c is >= '0' and <= '9') // enquanto for um digito numérico
                    {
                        number += c;
                        c = NextChar();
                    }

                    switch (c)
                    {
                        case 'B' or 'b': // byte forçado
                            result = ParseByte(lastPos, lastLine, number);
                            break;

                        case 'S' or 's': // short forçado
                            result = ParseShort(lastPos, lastLine, number);
                            break;

                        case 'L' or 'l': // long forçado
                            result = ParseLong(lastPos, lastLine, number);
                            break;

                        case 'F' or 'f': // float forçado
                            result = ParseFloat(lastPos, lastLine, number);
                            break;

                        case 'E' or 'e': // notação científica
                            number += 'E';

                            c = NextChar();
                            if (c is '+' or '-')
                                number += c;
                            else
                                Undo();

                            c = NextChar();
                            while (c is >= '0' and <= '9') // enquanto for um digito numérico
                            {
                                number += c;
                                c = NextChar();
                            }

                            if (c is 'F' or 'f') // float forçado
                            {
                                result = ParseFloat(lastPos, lastLine, number);
                            }
                            else
                            {
                                Undo();

                                result = ParseDouble(lastPos, lastLine, number);
                            }

                            break;

                        case '.': // separador decimal (pode ser um float ou double)
                            number += '.';
                            c = NextChar();
                            while (c is >= '0' and <= '9') // enquanto for um digito numérico
                            {
                                number += c;
                                c = NextChar();
                            }

                            if (c is 'F' or 'f') // float forçado
                            {
                                result = ParseFloat(lastPos, lastLine, number);
                            }
                            else if (c is 'E' or 'e') // notação científica
                            {
                                number += 'E';

                                c = NextChar();
                                if (c is '+' or '-')
                                    number += c;
                                else
                                    Undo();

                                c = NextChar();
                                while (c is >= '0' and <= '9') // enquanto for um digito numérico
                                {
                                    number += c;
                                    c = NextChar();
                                }

                                if (c is 'F' or 'f') // float forçado
                                {
                                    result = ParseFloat(lastPos, lastLine, number);
                                }
                                else
                                {
                                    Undo();

                                    result = ParseDouble(lastPos, lastLine, number);
                                }
                            }
                            else
                            {
                                Undo();

                                result = ParseDouble(lastPos, lastLine, number);
                            }

                            break;

                        default:
                            Undo();

                            result = ParseInt(lastPos, lastLine, number);
                            break;
                    }

                    break;
                }

                case '\'': // delimitador de caractere
                {
                    c = NextChar();
                    if (c == '\0')
                    {
                        Undo();
                        throw new CompilerException(CurrentInterval(lastPos, lastLine), "Caractere esperado mas fim do arquivo encontrado.");
                    }
                    else
                    {
                        Undo();
                    }

                    char ch = ParseChar(lastPos, lastLine, false);

                    c = NextChar();
                    if (c != '\'')
                    {
                        Undo();
                        throw new CompilerException(CurrentInterval(lastPos, lastLine), "Delimitador de caractere esperado.");
                    }

                    result = new CharLiteral(CurrentInterval(lastPos, lastLine), ch);
                    break;
                }

                case '"': // delimitador de uma cadeia de caracteres (string)
                {
                    string str = "";
                    while (true)
                    {
                        c = NextChar();
                        if (c == '"')
                            break;

                        Undo();

                        char ch = ParseChar(lastPos, lastLine, true);
                        str += ch;
                    }

                    result = new StringLiteral(CurrentInterval(lastPos, lastLine), str);
                    break;
                }

                default:
                    if (Symbol.IsSymbol(c))
                    {
                        string lastSymbol = c.ToString();
                        while (c != '\0')
                        {
                            c = NextChar();
                            string symbol = lastSymbol + c;
                            if (!Symbol.IsSymbol(symbol))
                                break;

                            lastSymbol = symbol;
                        }

                        Undo();

                        result = new Symbol(CurrentInterval(lastPos, lastLine), lastSymbol);
                    }
                    else if (Identifier.IsLetter(c)) // pode ser uma variável, uma constante ou uma função
                    {
                        string name = c + "";
                        c = NextChar();
                        while (c == '_' || Identifier.IsLetter(c) || c >= '0' && c <= '9') // uma variável, constante ou função pode conter somente letras ou _ (mas não iniciar com _)
                        {
                            name += c;
                            c = NextChar();
                        }

                        Undo();

                        result = Keyword.IsKeyword(name) ? new Keyword(CurrentInterval(lastPos, lastLine), name) : new Identifier(CurrentInterval(lastPos, lastLine), name);
                    }
                    else
                    {
                        Undo();
                        throw new CompilerException(CurrentInterval(lastPos, lastLine), $"Caractere inválido: {c}");
                    }

                    break;
            }
        }

        if (tokenCount < MAX_CACHED_TOKENS)
        {
            tokens[++tokenIndex] = result;
            tokenCount++;
        }
        else
        {
            Array.Copy(tokens, 1, tokens, 0, tokens.Length - 1);
            tokens[tokens.Length - 1] = result;
        }

        return result;
    }

    public bool IsNextToken()
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();
            return false;
        }

        return true;
    }

    public NumericLiteral NextNumber(bool throwException = true, string errorMessage = null)
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(CurrentInterval(), errorMessage ?? "Número esperado mas fim do arquivo encontrado.")
                : null;
        }

        if (token is not NumericLiteral)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"Número esperado mas {token} encontrado.")
                : null;
        }

        return (NumericLiteral) token;
    }

    public bool IsNextNumber()
    {
        return NextNumber(false) != null;
    }

    public Symbol NextSymbol(bool throwException = true, string errorMessage = null)
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(CurrentInterval(), errorMessage ?? "Símbolo esperado mas fim do arquivo encontrado.")
                : null;
        }

        if (token is not Symbol)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"Símbolo esperado mas {token} encontrado.")
                : null;
        }

        return (Symbol) token;
    }

    public bool IsNextSymbol()
    {
        return NextSymbol(false) != null;
    }

    public Symbol NextSymbol(string expectedValue, bool throwException = true, string errorMessage = null)
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(CurrentInterval(), errorMessage ?? $"'{expectedValue}' esperado mas fim da expressão encontrado.")
                : null;
        }

        if (token is not Symbol)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"'{expectedValue}' esperado mas {token} encontrado.")
                : null;
        }

        var symbol = (Symbol) token;
        if (symbol.Value != expectedValue)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"'{expectedValue}' esperado mas '{symbol.Value}' encontrado.")
                : null;
        }

        return symbol;
    }

    public bool IsNextSymbol(string expectedValue)
    {
        return NextSymbol(expectedValue, false) != null;
    }

    public Keyword NextKeyword(bool throwException = true, string errorMessage = null)
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(CurrentInterval(), errorMessage ?? "Palavra reservada esperada mas fim do arquivo encontrado.")
                : null;
        }

        if (token is not Keyword)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"Palavra reservada esperada mas {token} encontrado.")
                : null;
        }

        return (Keyword) token;
    }

    public bool IsNextKeyword()
    {
        return NextKeyword(false) != null;
    }

    public Keyword NextKeyword(string expectedValue, bool throwException = true, string errorMessage = null)
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(CurrentInterval(), errorMessage ?? $"'{expectedValue}' esperado mas fim do arquivo encontrado.")
                : null;
        }

        if (token is not Keyword)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"'{expectedValue}' esperado mas {token} encontrado.")
                : null;
        }

        var keyword = (Keyword) token;
        if (keyword.Value != expectedValue)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"'{expectedValue}' esperado mas '{keyword.Value}' encontrado.")
                : null;
        }

        return keyword;
    }

    public bool IsNextKeyword(string expectedValue)
    {
        return NextKeyword(expectedValue, false) != null;
    }

    public Identifier NextIdentifier(bool throwException = true, string errorMessage = null)
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(CurrentInterval(), errorMessage ?? "Identificador esperado mas fim do arquivo encontrado.")
                : null;
        }

        if (token is not Identifier)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"Identificador esperado mas {token} encontrado.")
                : null;
        }

        return (Identifier) token;
    }

    public bool IsNextIdentifier()
    {
        return NextIdentifier(false) != null;
    }

    public Identifier NextIdentifier(string expectedValue, bool throwException = true, string errorMessage = null)
    {
        var token = NextToken();
        if (token == null)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(CurrentInterval(), errorMessage ?? $"'{expectedValue}' mas fim do arquivo encontrado.")
                : null;
        }

        if (token is not Symbol)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"'{expectedValue}' esperado mas {token} encontrado.")
                : null;
        }

        var variable = (Identifier) token;
        if (variable.Name != expectedValue)
        {
            PreviusToken();

            return throwException
                ? throw new CompilerException(token.Interval, errorMessage ?? $"'{expectedValue}' esperado mas '{variable.Name}' encontrado.")
                : null;
        }

        return variable;
    }

    public bool IsNextIdentifier(string expectedValue)
    {
        return NextIdentifier(expectedValue, false) != null;
    }

    public Token PreviusToken()
    {
        if (tokenIndex < 0)
            return null;

        --tokenIndex;
        return tokenIndex < 0 ? null : tokens[tokenIndex];
    }
}
