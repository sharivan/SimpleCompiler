using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public class Lexer
    {
        private string input;
        private int pos;

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
                c = input[pos++];

            pos--;
        }

        public Token NextToken()
        {
            if (tokenIndex < tokens.Count - 1)
                return tokens[++tokenIndex];

            if (pos >= input.Length)
                return null;

            SkipBlanks();

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

                    if (c == '.')
                    {
                        number += '.';
                        c = input[pos++];
                        while (c >= '0' && c <= '9') // enquanto for um digito numérico
                        {
                            number += c;
                            c = input[pos++];
                        }

                        pos--;

                        try
                        {
                            float value = float.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
                            result = new DoubleLiteral(value);
                        }
                        catch (FormatException e)
                        {
                            throw new ParserException("Invalid float literal format: " + number, e);
                        }
                        catch (OverflowException e)
                        {
                            throw new ParserException("Overflow on converting float literal: " + number, e);
                        }
                    }
                    else
                    {
                        pos--;

                        try
                        {
                            int value = int.Parse(number);
                            result = new IntLiteral(value);
                        }
                        catch (FormatException e)
                        {
                            throw new ParserException("Invalid int literal format: " + number, e);
                        }
                        catch (OverflowException e)
                        {
                            throw new ParserException("Overflow on converting int literal: " + number, e);
                        }
                    }
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
                    throw new ParserException("Invalid character: " + c);
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
                    throw new ParserException(errorMessage != null ? errorMessage : "Number expected but end of expression found.");

                return null;
            }

            if (!(token is NumericLiteral))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Number expected but " + token + " found.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "Symbol expected but end of expression found.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Symbol expected but " + token + " found.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but end of expression found.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but " + token + " found.");

                return null;
            }

            Symbol symbol = (Symbol)token;
            if (symbol.Value != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but '" + symbol.Value + "' found.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "Keyword expected but end of expression found.");

                return null;
            }

            if (!(token is Keyword))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Keyword expected but " + token + " found.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but end of expression found.");

                return null;
            }

            if (!(token is Keyword))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but " + token + " found.");

                return null;
            }

            Keyword keyword = (Keyword)token;
            if (keyword.Value != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but '" + keyword.Value + "' found.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "Variable expected but end of expression found.");

                return null;
            }

            if (!(token is Identifier))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "Variable expected but " + token + " found.");

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
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' but end of expression found.");

                return null;
            }

            if (!(token is Symbol))
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but " + token + " found.");

                return null;
            }

            Identifier variable = (Identifier)token;
            if (variable.Name != expectedValue)
            {
                PreviusToken();

                if (throwException)
                    throw new ParserException(errorMessage != null ? errorMessage : "'" + expectedValue + "' expected but '" + variable.Name + "' found.");

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
