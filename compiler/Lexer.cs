using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
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
            get
            {
                return input;
            }

            set
            {
                input = value + '\0';
                tokens.Clear();
                tokenIndex = -1;
                pos = 0;
            }
        }

        public Lexer() : this("")
        {
        }

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
                            throw new ParserException("Fim do comentário esperado mas fim do arquivo encontrado.");
                        }

                        if (c == '*')
                        {
                            c = input[pos++];

                            if (c == '\0')
                            {
                                pos--;
                                throw new ParserException("Fim do comentário esperado mas fim do arquivo encontrado.");
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

        private char ParseChar(bool fromString)
        {
            char c = input[pos++];
            if (c == '\0')
            {
                pos--;
                throw new ParserException("Caractere esperado mas fim do arquivo encontrado.");
            }

            if (c == '\n' || c == '\r')
            {
                pos--;
                throw new ParserException("Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
            }

            if (c == '\\')
            {
                c = input[pos++];
                if (c == '\0')
                {
                    pos--;
                    throw new ParserException("Código de escape esperado mas fim do arquivo encontrado.");
                }

                if (c == '\n' || c == '\r')
                {
                    pos--;
                    throw new ParserException("Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
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
                                throw new ParserException("Código unicode esperado mas fim do arquivo encontrado.");
                            }

                            if (c == '\n' || c == '\r')
                            {
                                pos--;
                                throw new ParserException("Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
                            }

                            if (!(c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f'))
                            {
                                pos--;
                                break;
                            }
                        }

                        if (str == "")
                            throw new ParserException("Código unicode esperado.");

                        int hex;
                        try
                        {
                            hex = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        }
                        catch (FormatException)
                        {
                            throw new ParserException("Formato de código unicode inválido.");
                        }
                        catch (OverflowException)
                        {
                            throw new ParserException("Estouro na conversão de código unicode.");
                        }

                        if (hex < 0 || hex >= short.MaxValue)
                            throw new ParserException("Código unicode fora da faixa.");

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
                                throw new ParserException("Código octal esperado mas fim do arquivo encontrado.");
                            }

                            if (c == '\n' || c == '\r')
                            {
                                pos--;
                                throw new ParserException("Delimitador de " + (fromString ? "string" : "caractere") + " esperado mas quebra de linha encontrada.");
                            }

                            if (!(c >= '0' && c <= '7'))
                            {
                                pos--;
                                break;
                            }
                        }

                        if (str == "")
                            throw new ParserException("Código octal esperado.");

                        int oct;
                        try
                        {
                            oct = Convert.ToInt32(str, 8);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new ParserException("Código octal fora da faixa.");
                        }
                        catch (FormatException)
                        {
                            throw new ParserException("Formato de código octal inválido.");
                        }
                        catch (OverflowException)
                        {
                            throw new ParserException("Estouro na conversão de código octal.");
                        }

                        if (oct < 0 || oct >= short.MaxValue)
                            throw new ParserException("Código octal fora da faixa.");

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
                throw new ParserException("Código de escape inválido.");
            }

            if (c == '\'')
            {
                if (!fromString)
                {
                    pos--;
                    throw new ParserException("Caractere esperado.");
                }

                return '\'';
            }

            if (c == '"')
            {
                if (fromString)
                {
                    pos--;
                    throw new ParserException("Caractere esperado.");
                }

                return '"';
            }

            return c;
        }

        private ByteLiteral ParseByte(string number)
        {
            try
            {
                byte value = byte.Parse(number);
                return new ByteLiteral(value);
            }
            catch (FormatException e)
            {
                throw new ParserException("Formato de literal byte inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new ParserException("Overflow durante a conversão do literal byte: " + number, e);
            }
        }

        private ShortLiteral ParseShort(string number)
        {
            try
            {
                short value = short.Parse(number);
                return new ShortLiteral(value);
            }
            catch (FormatException e)
            {
                throw new ParserException("Formato de literal inteiro curto inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new ParserException("Overflow durante a conversão do literal inteiro curto: " + number, e);
            }
        }

        private IntLiteral ParseInt(string number)
        {
            try
            {
                int value = int.Parse(number);
                return new IntLiteral(value);
            }
            catch (FormatException e)
            {
                throw new ParserException("Formato de literal inteiro inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new ParserException("Overflow durante a conversão do literal inteiro: " + number, e);
            }
        }

        private LongLiteral ParseLong(string number)
        {
            try
            {
                long value = long.Parse(number);
                return new LongLiteral(value);
            }
            catch (FormatException e)
            {
                throw new ParserException("Formato de literal inteiro longo inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new ParserException("Overflow durante a conversão do literal inteiro longo: " + number, e);
            }
        }

        private FloatLiteral ParseFloat(string number)
        {
            try
            {
                float value = float.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
                return new FloatLiteral(value);
            }
            catch (FormatException e)
            {
                throw new ParserException("Formato de literal de ponto flutuante inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new ParserException("Overflow durante a conversão do literal de ponto flutuante: " + number, e);
            }
        }

        private DoubleLiteral ParseDouble(string number)
        {
            try
            {
                double value = double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
                return new DoubleLiteral(value);
            }
            catch (FormatException e)
            {
                throw new ParserException("Formato de literal de ponto flutuante inválido: " + number, e);
            }
            catch (OverflowException e)
            {
                throw new ParserException("Overflow durante a conversão do literal de ponto flutuante: " + number, e);
            }
        }

        public Token NextToken()
        {
            if (tokenIndex < tokens.Count - 1)
                return tokens[++tokenIndex];

            if (pos >= input.Length)
                return null;

            SkipBlanks();
            SkipComments();

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
                        result = ParseByte(number);
                    else if (c == 'S' || c == 's')
                        result = ParseShort(number);
                    else if (c == 'L' || c == 'l')
                        result = ParseLong(number);
                    else if (c == 'F' || c == 'f')
                        result = ParseFloat(number);
                    else if (c == 'E' || c == 'e')
                    {
                        number += 'E';
                        c = input[pos++];
                        while (c >= '0' && c <= '9') // enquanto for um digito numérico
                        {
                            number += c;
                            if (pos >= input.Length)
                                break;

                            c = input[pos++];
                        }

                        if (c == 'F' || c == 'f')
                            result = ParseFloat(number);
                        else
                        {
                            pos--;

                            result = ParseDouble(number);
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
                            result = ParseFloat(number);
                        else if (c == 'E' || c == 'e')
                        {
                            number += 'E';
                            c = input[pos++];
                            while (c >= '0' && c <= '9') // enquanto for um digito numérico
                            {
                                number += c;
                                if (pos >= input.Length)
                                    break;

                                c = input[pos++];
                            }

                            if (c == 'F' || c == 'f')
                                result = ParseFloat(number);
                            else
                            {
                                pos--;

                                result = ParseDouble(number);
                            }
                        }
                        else
                        {
                            pos--;

                            result = ParseDouble(number);
                        }
                    }
                    else
                    {
                        pos--;

                        result = ParseInt(number);
                    }
                }
                else if (c == '\'')
                {
                    c = input[pos++];
                    if (c == '\0')
                    {
                        pos--;
                        throw new ParserException("Caractere esperado mas fim do arquivo encontrado.");
                    }
                    else
                        pos--;

                    char ch = ParseChar(false);

                    c = input[pos++];
                    if (c != '\'')
                    {
                        pos--;
                        throw new ParserException("Delimitador de caractere esperado.");
                    }

                    result = new CharLiteral(ch);
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

                        char ch = ParseChar(true);
                        str += ch;
                    }

                    result = new StringLiteral(str);
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

                    result = new Symbol(lastSymbol);
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
                        result = new Keyword(name);
                    else
                        result = new Identifier(name);
                }
                else
                    throw new ParserException("Caractere inválido: " + c);
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
                    throw new ParserException(errorMessage != null ? errorMessage : "Número esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is NumericLiteral))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Número esperado mas " + token + " encontrado.");

                return null;
            }

            return (NumericLiteral)token;
        }

        public Symbol NextSymbol(bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Símbolo esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Símbolo esperado mas " + token + " encontrado.");

                return null;
            }

            return (Symbol)token;
        }

        public Symbol NextSymbol(string expectedValue, bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas fim da expressão encontrado.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas " + token + " encontrado.");

                return null;
            }

            Symbol symbol = (Symbol)token;
            if (symbol.Value != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas '" + symbol.Value + "' encontrado.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "Palavra reservada esperada mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Keyword))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Palavra reservada esperada mas " + token + " encontrado.");

                return null;
            }

            return (Keyword)token;
        }

        public Keyword NextKeyword(string expectedValue, bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Keyword))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas " + token + " encontrado.");

                return null;
            }

            Keyword keyword = (Keyword)token;
            if (keyword.Value != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas '" + keyword.Value + "' encontrado.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "Identificador esperado mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Identifier))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Identificador esperado mas " + token + " encontrado.");

                return null;
            }

            return (Identifier)token;
        }

        public Identifier NextIdentifier(string expectedValue, bool throwException = true, string errorMessage = null)
        {
            Token token = NextToken();
            if (token == null)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' mas fim do arquivo encontrado.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas " + token + " encontrado.");

                return null;
            }

            Identifier variable = (Identifier)token;
            if (variable.Name != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' esperado mas '" + variable.Name + "' encontrado.");

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
