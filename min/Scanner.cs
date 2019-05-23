using System.Collections.Generic;

namespace min
{
    public class Scanner
    {
        private static readonly Dictionary<string, TokenType> reservedKeywords = new Dictionary<string, TokenType> {
            { "true", TokenType.TRUE },
            { "false", TokenType.FALSE },
            { "null", TokenType.NULL },
            { "var", TokenType.VAR },
            { "if", TokenType.IF },
            { "else", TokenType.ELSE },
            { "for", TokenType.FOR },
            { "while", TokenType.WHILE },
            { "class", TokenType.CLASS },
            { "function", TokenType.FUNCTION },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "return", TokenType.RETURN },
            { "print", TokenType.PRINT },
        };

        public readonly string source;

        public readonly List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
        }

        #region Scanning

        public List<Token> ScanTokens()
        {
            while (IsAtEnd() == false)
            {
                start = current;
                ScanToken();
            }

            // Add EOF at the end
            tokens.Add(new Token(TokenType.EOF, "", null, line));

            return tokens;
        }

        /// <summary>
        ///  Move through the characters to scan one token and add it to the list of tokens.
        /// </summary>
        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '+': AddToken(TokenType.PLUS); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

                case '&':
                    if (Match('&'))
                        AddToken(TokenType.AND);
                    else
                        // TODO: Bitwise "&" does not exist yet.
                        Min.Error(line, $"Unexpected character \"{c}\"");
                    break;
                case '|':
                    if (Match('|'))
                        AddToken(TokenType.OR);
                    else
                        // TODO: Bitwise "|" does not exist yet.
                        Min.Error(line, $"Unexpected character \"{c}\"");
                    break;

                case '/':
                    // Comment //
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line, keep consuming characters until then.
                        while (Peek() != '\n' && IsAtEnd() == false) Advance();
                    }
                    // Block comment /*
                    else if (Match('*')) SkipBlockComment();
                    // Any other slash /
                    else AddToken(TokenType.SLASH);

                    break;

                case '"':
                    ReadString();
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace
                    break;

                case '\n':
                    line++;
                    break;

                default:
                    // Have to go through default because bools/methods don't work in switch/case
                    if (IsDigit(c)) ReadNumber();
                    else if (IsAlpha(c)) ReadIdentifier();
                    else
                        Min.Error(line, $"Unexpected character \"{c}\"");
                    break;
            }
        }

        /// <summary>
        /// Get text of the current lexeme, create a token and add it to the list of tokens.
        /// </summary>
        /// <param name="type">The TokenType of the new token to add.</param>
        /// <param name="literal">The literal value of the token.</param>
        private void AddToken(TokenType type, object literal = null)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        #endregion

        #region Scanning Helpers

        /// <summary>
        /// Consume the next character and returns it.
        /// </summary>
        /// <returns>The character at the next position.</returns>
        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        /// <summary>
        /// Get the character at the current position without consuming it.
        /// </summary>
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        /// <summary>
        /// Get the character at the next position without consuming it.
        /// </summary>
        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        /// <summary>
        /// Consume the next character if it matches the expected one.
        /// (Kind of a conditional Advance).
        /// </summary>
        /// <param name="expected">Expected character at the next position.</param>
        /// <returns>True if the character was correct.</returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;

            // If it wasn't the expected character, false
            if (source[current] != expected) return false;

            // If it was the expected character, move to the next one (consume), true
            current++;
            return true;
        }

        #endregion

        #region Advanced Scanning

        /// <summary>
        /// Read a full string between quotes " and add it as a token.
        /// </summary>
        private void ReadString()
        {
            // Keep moving through the string until it reaches "
            while (Peek() != '"' && IsAtEnd() == false)
            {
                // Allow multi-line strings
                if (Peek() == '\n') line++;

                Advance();
            }

            // Unterminated string
            if (IsAtEnd())
            {
                Min.Error(line, "Unterminated string.");
                return;
            }

            // Move past the closing quote (")
            Advance();

            // Trim the surrounding quotes
            string value = source.Substring(start + 1, current - 2 - start);
            // TODO: Support escape sequences (unescape them here)

            AddToken(TokenType.STRING, value);
        }

        /// <summary>
        /// Read a full number (including decimal) and add it as a token.
        /// </summary>
        private void ReadNumber()
        {
            // Move through every digit until something else is found
            while (IsDigit(Peek())) Advance();

            // If a dot is found, it's a decimal with fractional part
            if (Peek() == '.' && IsDigit(PeekNext()))
                // Consume the "."
                Advance();

            // Move through the rest of digits
            while (IsDigit(Peek())) Advance();

            AddToken(TokenType.NUMBER, double.Parse(source.Substring(start, current - start)));
        }

        /// <summary>
        /// Read an identifier or reserved keyword and add it as a token.
        /// </summary>
        private void ReadIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = source.Substring(start, current - start);

            TokenType type = reservedKeywords.ContainsKey(text)
                ? reservedKeywords[text]
                : TokenType.IDENTIFIER;

            AddToken(type);
        }


        /// <summary>
        /// Skip through a block comment.
        /// </summary>
        private void SkipBlockComment()
        {
            // Stop when (Peek == * && PeekNext == /) ---> (Peek != * || PeekNext != /)
            // Keep moving through the comment until it reaches */
            while ((Peek() != '*' || PeekNext() != '/') && IsAtEnd() == false)
            {
                // Allow multi-line block comments
                if (Peek() == '\n') line++;

                Advance();
            }

            // Unterminated comment
            if (IsAtEnd())
            {
                Min.Error(line, "Unterminated block comment.");
                return;
            }

            // Move past the closing comment */
            Advance();
            Advance();
        }

        #endregion

        #region Helpers 

        /// <summary>
        /// Whether or not the Scanner has reached the end of the file
        /// </summary>
        private bool IsAtEnd()
        {
            return current >= source.Length;
        }

        /// <summary>
        /// Whether or not the given character is a digit.
        /// </summary>
        /// <param name="c">The character to check for.</param>
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Whether or not the given character is a letter or underscore.
        /// </summary>
        /// <param name="c">The character to check for.</param>
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
             (c >= 'A' && c <= 'Z') ||
              c == '_';
        }

        /// <summary>
        /// Whether or not the given character is a letter, underscore, or number
        /// </summary>
        /// <param name="c">The character to check for.</param>
        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        #endregion
    }
}