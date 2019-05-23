using System;
using System.Collections.Generic;

namespace min
{
    public class Parser
    {
        private class ParseError : Exception { }

        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public IExpression Parse()
        {
            // TODO: See Challenges http://craftinginterpreters.com/parsing-expressions.html#challenges
            try
            {
                return Expression();
            }
            // Temporary mode to exit out of panic mode on Parse Error.
            catch (ParseError error)
            {
                return null;
            }
        }

        #region Expressions

        /**
            expression → literal | unary | binary | grouping ;
            literal    → NUMBER | STRING | "false" | "true" | "nil" ;
            grouping   → "(" expression ")" ;
            unary      → ( "-" | "!" ) expression ;
            binary     → expression operator expression ;
            operator   → "==" | "!=" | "<" | "<=" | ">" | ">=" | "+"  | "-"  | "*" | "/" ;
         */

        private IExpression Expression()
        {
            return Equality();
        }

        /// <summary>
        /// Get an expression which might contain an equality comparison (== or !=).
        /// </summary>
        private IExpression Equality()
        {
            IExpression expression = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                IExpression right = Comparison();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }

        /// <summary>
        /// Get an expression which might contain a comparison (<, >, <=, >=).
        /// </summary>
        private IExpression Comparison()
        {
            IExpression expression = Addition();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                IExpression right = Addition();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }

        /// <summary>
        /// Get an expression which might contain an addition (+, -).
        /// </summary>
        private IExpression Addition()
        {
            IExpression expression = Multiplication();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token op = Previous();
                IExpression right = Multiplication();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }

        /// <summary>
        /// Get an expression which might contain a multiplication (*, /).
        /// </summary>
        private IExpression Multiplication()
        {
            IExpression expression = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = Previous();
                IExpression right = Unary();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }

        /// <summary>
        /// Get an expression which might have unary operators (!, -)
        /// </summary>
        private IExpression Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                IExpression right = Unary();
                return new UnaryExpression(op, right);
            }

            return Primary();
        }

        /// <summary>
        /// Get a primary expression (bool, string, number, group, null)
        /// </summary>
        private IExpression Primary()
        {
            if (Match(TokenType.FALSE)) return new LiteralExpression(false);
            if (Match(TokenType.TRUE)) return new LiteralExpression(true);
            if (Match(TokenType.NULL)) return new LiteralExpression(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new LiteralExpression(Previous().literal);
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                IExpression expression = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new GroupingExpression(expression);
            }

            // Expected an expression but found something else
            throw Error(Peek(), "Expect expression.");
        }

        #endregion

        /// <summary>
        /// Try to synchronize the parser after an error by moving until it reaches the next statement (after a semicolon or keyword tokens).
        /// </summary>
        private void Synchronize()
        {
            Advance();

            while (IsAtEnd() == false)
            {
                if (Previous().type == TokenType.SEMICOLON) return;

                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUNCTION:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }
            }

            Advance();
        }

        #region Helpers

        /// <summary>
        /// Get the current token in the list, without consuming it.
        /// </summary>
        private Token Peek()
        {
            return tokens[current];
        }

        /// <summary>
        /// ///  Get the previous token in the list.
        /// </summary>
        private Token Previous()
        {
            return tokens[current - 1];
        }

        /// <summary>
        ///  Get and consume the current token.
        /// </summary>
        /// <returns>The token that was consumed.</returns>
        private Token Advance()
        {
            if (IsAtEnd() == false) current++;
            return Previous();
        }

        /// <summary>
        ///  Check if the current token is of the given type, without consuming it.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <returns>True if the current token is correct.</returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;

            return Peek().type == type;
        }

        /// <summary>
        /// Checks if the current token is any of the given types, consumes it if true.
        /// </summary>
        /// <param name="types">The types to check for.</param>
        /// <returns>True if the next token was correct and has been consumed.</returns>
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to consume an expected token, if incorrect token is found, throw a parsing error.
        /// </summary>
        /// <param name="type">The type of the expected token.</param>
        /// <param name="message">The error message to output if incorrect.</param>
        /// <returns>The correct token.</returns>
        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            /* 
              Throw on Error so that the Parser can re-synchronize to a place where there is no error.
              Simply catch the error where the parser should re-synchronize.
              This allows the parser to keep going after a parsing error and try to parse the rest of the code.
            */
            throw Error(Peek(), message);
        }

        /// <summary>
        /// Whether or not the parser has reached the end of the token list.
        /// </summary>
        /// <returns>True if it has reached the end.</returns>
        private bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }

        #endregion

        /// <summary>
        /// Report a Parsing Error at a specific token.
        /// </summary>
        /// <param name="token">The token that encountered an error.</param>
        /// <param name="message">The message of the error.</param>
        /// <returns>The Parsing Error created as a result.</returns>
        private ParseError Error(Token token, string message)
        {
            Min.Error(token, message);
            return new ParseError();
        }
    }
}