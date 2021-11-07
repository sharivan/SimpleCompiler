using System;
using System.Collections.Generic;
using System.Text;

namespace compiler.lexer
{
    public class Lexer
    {
        private string input;
        private int pos;
        private int line;

        private List<Token> tokens;
        private int tokenIndex;

        public string Input
        {
            get => input;

            set
            {
                tokens.Clear();

                pos = 0;
                line = 1;
                tokenIndex = -1;

                input = value + '\0';
            }
        }

        public int CurrentPos => pos;

        public int CurrentLine => line;

        public Lexer() : this("")
        {
        }

        public SourceInterval CurrentInterval(int startPos) => new SourceInterval(startPos, pos, line);

        public Lexer(string input)
        {
            tokens = new List<Token>();

            Input = input;
        }

        public Token CurrentToken()
        {
            if (tokens.Count == 0)
                return null;

            return tokens[tokenIndex];
        }

        private void SkipBlanks()
        {
            char c = input[pos++];
            if (c == '\0')
            {
                pos--;
                return;
            }

            while (c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                if (c == '\n')
                    line++;

                c = input[pos++];
            }

            pos--;
        }

        private void SkipComments()
        {
            int startPos = pos;
            char c = input[pos++];
            if (c == '\0')
            {
                pos--;
                return;
            }

            if (c == '/')
            {
                c = input[pos++];
                if (c == '/')
                {
                    while (true)
                    {
                        c = input[pos++];
                        if (c == '\0')
                        {
                            pos--;
                            return;
                        }

                        if (c == '\n')
                        {
                            line++;
                            return;
                        }
                    }
                }
                else if (c == '*')
                {
                    while (true)
                    {
                        c = input[pos++];

                        if (c == '\0')
                        {
                            pos--;
                            throw new CompilerException(CurrentInterval(startPos), "Fim do comentário esperado mas fim do arquivo encontrado.");
                        }

                        if (c == '*')
                        {
                            c = input[pos++];

                            if (c == '\0')
                            {
                                pos--;
                                throw new CompilerException(CurrentInterval(startPos), "Fim do comentário esperado mas fim do arquivo encontrado.");
                            }

                            if (c == '/')
                                return;
                        }
                    }
                }
                else
                    pos -= 2;
            }
            else
                pos--;
        }

        private char ParseChar(int startPos, bool fromString)
        {
            int charStartPos = pos;
            char c = input[pos++];
            if (c == '\0')
            {
                pos--;
                throw new CompilerException(CurrentInterval(startPos), "Caractere esperado mas fim do arquivo encontrado.");
            }

            if (c == '\n' || c == '\r')
            {
                pos--;
                throw new CompilerException(CurrentInterval(startPos), "Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
            }

            if (c == '\\')
            {
                c = input[pos++];
                if (c == '\0')
                {
                    pos--;
                    throw new CompilerException(CurrentInterval(charStartPos), "Código de escape esperado mas fim do arquivo encontrado.");
                }

                if (c == '\n' || c == '\r')
                {
                    pos--;
                    throw new CompilerException(CurrentInterval(charStartPos), "Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
                }

                switch (c)
                {
                    case 'n':
                        return '\n';

                    case 'r':
                        return '\r';

                    case 't':
                        return '\t';

                    case 'u':
                    {
                        string str = "";
                        while (true)
                        {
                            c = input[pos++];
                            if (c == '\0')
                            {
                                pos--;
                                throw new CompilerException(CurrentInterval(charStartPos), "Código unicode esperado mas fim do arquivo encontrado.");
                            }

                            if (c == '\n' || c == '\r')
                            {
                                pos--;
                                throw new CompilerException(CurrentInterval(charStartPos), "Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
                            }

                            if (!(c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f'))
                            {
                                pos--;
                                break;
                            }
                        }

                        if (str == "")
                            throw new CompilerException(CurrentInterval(charStartPos), "Código unicode esperado.");

                        int hex;
                        try
                        {
                            hex = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        }
                        catch (FormatException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos), "Formato de código unicode inválido.");
                        }
                        catch (OverflowException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos), "Estouro na conversão de código unicode.");
                        }

                        if (hex < 0 || hex >= short.MaxValue)
                            throw new CompilerException(CurrentInterval(charStartPos), "Código unicode fora da faixa.");

                        return (char) hex;
                    }

                    case '0':
                    {
                        string str = "";
                        while (true)
                        {
                            c = input[pos++];
                            if (c == '\0')
                            {
                                pos--;
                                throw new CompilerException(CurrentInterval(charStartPos), "Código octal esperado mas fim do arquivo encontrado.");
                            }

                            if (c == '\n' || c == '\r')
                            {
                                pos--;
                                throw new CompilerException(CurrentInterval(charStartPos), "Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
                            }

                            if (!(c >= '0' && c <= '7'))
                            {
                                pos--;
                                break;
                            }
                        }

                        if (str == "")
                            throw new CompilerException(CurrentInterval(charStartPos), "Código octal esperado.");

                        int oct;
                        try
                        {
                            oct = Convert.ToInt32(str, 8);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos), "Código octal fora da faixa.");
                        }
                        catch (FormatException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos), "Formato de código octal inválido.");
                        }
                        catch (OverflowException)
                        {
                            throw new CompilerException(CurrentInterval(charStartPos), "Estouro na conversão de código octal.");
                        }

                        if (oct < 0 || oct >= short.MaxValue)
                            throw new CompilerException(CurrentInterval(charStartPos), "Código octal fora da faixa.");

                        return (char) oct;
                    }

                    case '\\':
                        return '\\';

                    case '\'':
                        return '\'';

                    case '"':
                        return '"';
                }

                pos--;
                throw new CompilerException(CurrentInterval(charStartPos), "Código de escape inválido.");
            }

            if (c == '\'')
            {
                if (!fromString)
                {
                    pos--;
                    throw new CompilerException(CurrentInterval(charStartPos), "Caractere esperado.");
                }

                return '\'';
            }

            return c;
        }

        private ByteLiteral ParseByte(int startPos, string number)
        {
            try
            {
                byte value = byte.Parse(number);
                return new ByteLiteral(CurrentInterval(startPos), value);
            }
            catch (FormatException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Formato de literal byte inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Overflow durante a conversão do literal byte: " + number, e);
            }
        }

        private ShortLiteral ParseShort(int startPos, string number)
        {
            try
            {
                short value = short.Parse(number);
                return new ShortLiteral(CurrentInterval(startPos), value);
            }
            catch (FormatException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Formato de literal inteiro curto inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Overflow durante a conversão do literal inteiro curto: " + number, e);
            }
        }

        private IntLiteral ParseInt(int startPos, string number)
        {
            try
            {
                int value = int.Parse(number);
                return new IntLiteral(CurrentInterval(startPos), value);
            }
            catch (FormatException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Formato de literal inteiro inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Overflow durante a conversão do literal inteiro: " + number, e);
            }
        }

        private LongLiteral ParseLong(int startPos, string number)
        {
            try
            {
                long value = long.Parse(number);
                return new LongLiteral(CurrentInterval(startPos), value);
            }
            catch (FormatException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Formato de literal inteiro longo inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Overflow durante a conversão do literal inteiro longo: " + number, e);
            }
        }

        private FloatLiteral ParseFloat(int startPos, string number)
        {
            try
            {
                float value = float.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
                return new FloatLiteral(CurrentInterval(startPos), value);
            }
            catch (FormatException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Formato de literal de ponto flutuante inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Overflow durante a conversão do literal de ponto flutuante: " + number, e);
            }
        }

        private DoubleLiteral ParseDouble(int startPos, string number)
        {
            try
            {
                double value = double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
                return new DoubleLiteral(CurrentInterval(startPos), value);
            }
            catch (FormatException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Formato de literal de ponto flutuante inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new CompilerException(CurrentInterval(startPos), "Overflow durante a conversão do literal de ponto flutuante: " + number, e);
            }
        }

        public Token NextToken()
        {
            if (tokenIndex < tokens.Count - 1)
                return tokens[++tokenIndex];

            if (pos >= input.Length)
                return null;

            int lastPos;
            while (true)
            {
                lastPos = pos;

                SkipBlanks();
                SkipComments();

                if (lastPos == pos)
                    break;

                lastPos = pos;
            }

            lastPos = pos;
            char c = input[pos++];

            Token result = null;

            if (c != '\0')
            {
                if (c >= '0' && c <= '9') // é um digito numérico?
                {
                    string number = c + "";
                    c = input[pos++];
                    while (c >= '0' && c <= '9') // enquanto for um digito numérico
                    {
                        number += c;
                        if (pos >= input.Length)
                            break;

                        c = input[pos++];
                    }

                    if (c == 'B' || c == 'b')
                        result = ParseByte(lastPos, number);
                    else if (c == 'S' || c == 's')
                        result = ParseShort(lastPos, number);
                    else if (c == 'L' || c == 'l')
                        result = ParseLong(lastPos, number);
                    else if (c == 'F' || c == 'f')
                        result = ParseFloat(lastPos, number);
                    else if (c == 'E' || c == 'e')
                    {
                        number += 'E';

                        c = input[pos++];
                        if (c == '+' || c == '-')
                            number += c;
                        else
                            pos--;

                        c = input[pos++];
                        while (c >= '0' && c <= '9') // enquanto for um digito numérico
                        {
                            number += c;
                            if (pos >= input.Length)
                                break;

                            c = input[pos++];
                        }

                        if (c == 'F' || c == 'f')
                            result = ParseFloat(lastPos, number);
                        else
                        {
                            pos--;

                            result = ParseDouble(lastPos, number);
                        }
                    }
                    else if (c == '.')
                    {
                        number += '.';
                        c = input[pos++];
                        while (c >= '0' && c <= '9') // enquanto for um digito numérico
                        {
                            number += c;
                            c = input[pos++];
                        }

                        if (c == 'F' || c == 'f')
                            result = ParseFloat(lastPos, number);
                        else if (c == 'E' || c == 'e')
                        {
                            number += 'E';

                            c = input[pos++];
                            if (c == '+' || c == '-')
                                number += c;
                            else
                                pos--;

                            c = input[pos++];
                            while (c >= '0' && c <= '9') // enquanto for um digito numérico
                            {
                                number += c;
                                if (pos >= input.Length)
                                    break;

                                c = input[pos++];
                            }

                            if (c == 'F' || c == 'f')
                                result = ParseFloat(lastPos, number);
                            else
                            {
                                pos--;

                                result = ParseDouble(lastPos, number);
                            }
                        }
                        else
                        {
                            pos--;

                            result = ParseDouble(lastPos, number);
                        }
                    }
                    else
                    {
                        pos--;

                        result = ParseInt(lastPos, number);
                    }
                }
                else if (c == '\'')
                {
                    c = input[pos++];
                    if (c == '\0')
                    {
                        pos--;
                        throw new CompilerException(CurrentInterval(lastPos), "Caractere esperado mas fim do arquivo encontrado.");
                    }
                    else
                        pos--;

                    char ch = ParseChar(lastPos, false);

                    c = input[pos++];
                    if (c != '\'')
                    {
                        pos--;
                        throw new CompilerException(CurrentInterval(lastPos), "Delimitador de caractere esperado.");
                    }

                    result = new CharLiteral(CurrentInterval(lastPos), ch);
                }
                else if (c == '"')
                {
                    string str = "";
                    while (true)
                    {
                        c = input[pos++];
                        if (c == '"')
                            break;
                        else
                            pos--;

                        char ch = ParseChar(lastPos, true);
                        str += ch;
                    }

                    result = new StringLiteral(CurrentInterval(lastPos), str);
                }
                else if (Symbol.IsSymbol(c))
                {
                    string lastSymbol = c.ToString();
                    while (c != '\0')
                    {
                        c = input[pos++];
                        string symbol = lastSymbol + c;
                        if (!Symbol.IsSymbol(symbol))
                            break;

                        lastSymbol = symbol;
                    }

                    pos--;

                    result = new Symbol(CurrentInterval(lastPos), lastSymbol);
                }
                else if (Identifier.IsLetter(c)) // pode ser uma variável, uma constante ou uma função
                {
                    string name = c + "";
                    c = input[pos++];
                    while (c == '_' || Identifier.IsLetter(c) || c >= '0' && c <= '9') // uma variável, constante ou função pode conter somente letras ou _ (mas não iniciar com _)
                    {
                        name += c;
                        c = input[pos++];
                    }

                    pos--;

                    if (Keyword.IsKeyword(name))
                        result = new Keyword(CurrentInterval(lastPos), name);
                    else
                        result = new Identifier(CurrentInterval(lastPos), name);
                }
                else
                    throw new CompilerException(CurrentInterval(lastPos), "Caractere inválido: " + c);
            }

            tokens.Add(result);
            tokenIndex = tokens.Count - 1;

            return result;
        }

        public NumericLiteral NextNumber(bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(CurrentInterval(pos), errorMessage != null ? errorMessage : "Número esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is NumericLiteral))
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "Número esperado mas " + token + " encontrado.");

                return null;
            }

            return (NumericLiteral) token;
        }

        public Symbol NextSymbol(bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(CurrentInterval(pos), errorMessage != null ? errorMessage : "Símbolo esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "Símbolo esperado mas " + token + " encontrado.");

                return null;
            }

            return (Symbol) token;
        }

        public Symbol NextSymbol(string expectedValue, bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(CurrentInterval(pos), errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas fim da expressão encontrado.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas " + token + " encontrado.");

                return null;
            }

            Symbol symbol = (Symbol) token;
            if (symbol.Value != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas '" + symbol.Value + "' encontrado.");

                return null;
            }

            return symbol;
        }

        public Keyword NextKeyword(bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(CurrentInterval(pos), errorMessage != null ? errorMessage : "Palavra reservada esperada mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Keyword))
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "Palavra reservada esperada mas " + token + " encontrado.");

                return null;
            }

            return (Keyword) token;
        }

        public Keyword NextKeyword(string expectedValue, bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(CurrentInterval(pos), errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Keyword))
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas " + token + " encontrado.");

                return null;
            }

            Keyword keyword = (Keyword) token;
            if (keyword.Value != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas '" + keyword.Value + "' encontrado.");

                return null;
            }

            return keyword;
        }

        public Identifier NextIdentifier(bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(CurrentInterval(pos), errorMessage != null ? errorMessage : "Identificador esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Identifier))
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "Identificador esperado mas " + token + " encontrado.");

                return null;
            }

            return (Identifier) token;
        }

        public Identifier NextIdentifier(string expectedValue, bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(CurrentInterval(pos), errorMessage != null ? errorMessage : "'" + expectedValue + "' mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas " + token + " encontrado.");

                return null;
            }

            Identifier variable = (Identifier) token;
            if (variable.Name != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new CompilerException(token.Interval, errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas '" + variable.Name + "' encontrado.");

                return null;
            }

            return variable;
        }

        public Token PreviusToken()
        {
            if (tokenIndex < 0)
                return null;

            --tokenIndex;
            if (tokenIndex < 0)
                return null;

            return tokens[tokenIndex];
        }
    }
}
