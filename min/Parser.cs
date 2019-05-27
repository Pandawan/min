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

        public List<IStatement> Parse()
        {
            List<IStatement> statements = new List<IStatement>();
            while (IsAtEnd() == false)
            {
                statements.Add(Declaration());
            }
            return statements;

            // TODO: See Challenges http://craftinginterpreters.com/parsing-expressions.html#challenges
        }

        #region Statements
        /**
            program     → declaration* EOF ;
            declaration → functionDecl | letDecl | statement ;
            funDecl     → "function" function ;
            function    → IDENTIFIER "(" parameters? ")" block ;
            parameters  → IDENTIFIER ( "," IDENTIFIER )* ;
            letDecl     → "let" IDENTIFIER ( "=" expression )? ";" ;
            exprStmt    → expression ";" ;
            statement   → exprStmt | ifStmt | printStmt | returnStmt | whileStmt | block ;
            forStmt     → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;
            ifStmt      → "if" "(" expression ")" statement ( "else" statement )? ;
            printStmt   → "print" expression ";" ;
            returnStmt  → "return" expression? ";" ;
            whileStmt   → "while" "(" expression ")" statement ;  
            block       → "{" declaration* "}" ;
         */

        private IStatement Declaration()
        {
            try
            {
                if (Match(TokenType.FUNCTION)) return FunctionStatement("function");
                if (Match(TokenType.LET)) return LetDeclaration();
                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        /// <summary>
        /// Get a statement.
        /// </summary>
        private IStatement Statement()
        {
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new BlockStatement(BlockStatement());

            return ExpressionStatement();
        }

        /// <summary>
        /// Get a for statement and compile it to a while (under the hood).
        /// </summary>
        private IStatement ForStatement()
        {
            /**
                Desugaring for loop into a while loop:

                initializer;
                while (condition) {
                    body;
                    increment;
                }
            */

            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            // First part of for loop, an expression/declaration that runs once
            IStatement initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.LET))
            {
                initializer = LetDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            // Second part of for loop, a condition that is checked on every iteration
            IExpression condition = null;
            if (Check(TokenType.SEMICOLON) == false)
            {
                condition = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            // Third part of for loop, an incrementer that runs every iteration
            IExpression increment = null;
            if (Check(TokenType.RIGHT_PAREN) == false)
            {
                increment = Expression();
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            IStatement body = Statement();

            // If there is an increment
            if (increment != null)
            {
                // Add the increment to the body of the while loop
                body = new BlockStatement(new List<IStatement>(){
                    body, new ExpressionStatement(increment)
                });
            }

            // Create a while loop with condition + body (which also has increment)
            if (condition == null) condition = new LiteralExpression(true);
            body = new WhileStatement(condition, body);

            // Lastly, add an initializer before the while loop
            if (initializer != null)
            {
                body = new BlockStatement(new List<IStatement>() {
                    initializer,
                    body
                });
            }

            return body;
        }

        /// <summary>
        /// Get an if statement (which might contain an else).
        /// </summary>
        private IStatement IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            IExpression condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition");

            IStatement thenBranch = Statement();
            IStatement elseBranch = Match(TokenType.ELSE) ? Statement() : null;

            return new IfStatement(condition, thenBranch, elseBranch);
        }

        /// <summary>
        /// Get a Print Statement (print <expression>;)
        /// </summary>
        private IStatement PrintStatement()
        {
            // TODO: Convert that to a library print call
            IExpression value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new PrintStatement(value);
        }

        private IStatement ReturnStatement()
        {
            Token keyword = Previous();
            IExpression value = null;
            if (Check(TokenType.SEMICOLON) == false)
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new ReturnStatement(keyword, value);
        }

        /// <summary>
        /// Get a while statament.
        /// </summary>
        private IStatement WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            IExpression condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            IStatement body = Statement();

            return new WhileStatement(condition, body);
        }

        /// <summary>
        /// Get an expression statement
        /// </summary>
        private IStatement ExpressionStatement()
        {
            IExpression expression = Expression();

            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new ExpressionStatement(expression);
        }

        /// <summary>
        /// Get a function (or class) statement.
        /// </summary>
        /// <param name="kind">What kind of statement this is (ie. function)</param>
        private IStatement FunctionStatement(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");

            List<Token> parameters = new List<Token>();
            if (Check(TokenType.RIGHT_PAREN) == false)
            {
                do
                {
                    if (parameters.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 parameters.");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
            List<IStatement> body = BlockStatement();

            return new FunctionStatement(name, parameters, body);
        }

        /// <summary>
        /// Get a list of declarations inside a block ( {} )
        /// </summary>
        private List<IStatement> BlockStatement()
        {
            List<IStatement> statements = new List<IStatement>();

            while (Check(TokenType.RIGHT_BRACE) == false && IsAtEnd() == false)
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        /// <summary>
        /// Get a let declaration (let <name> [= initializer])
        /// </summary>
        private IStatement LetDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            // Try to match an initializer, skip if none
            IExpression initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new LetStatement(name, initializer);
        }

        #endregion

        #region Expressions

        /**
            expression → assignment ;
            assignment → IDENTIFIER "=" assignment | logic_or ;
            logic_or   → logic_and ( "or" logic_and )* ;
            logic_and  → equality ( "and" equality )* ;


            expression → literal | unary | binary | grouping | ternary ;
            literal    → NUMBER | STRING | "false" | "true" | "null" | IDENTIFIER ;
            grouping   → "(" expression ")" ;
            unary      → ( "-" | "!" ) expression | call ;
            call       → primary ( "(" arguments? ")" )* ;
            arguments  → expression ( "," expression )* ;
            binary     → expression operator expression ;
            operator   → "==" | "!=" | "<" | "<=" | ">" | ">=" | "+"  | "-"  | "*" | "/" ;
            ternary    → condition "?" thenExpression ":" elseExpression ;
         */

        private IExpression Expression()
        {
            return Comma();
        }

        /// <summary>
        /// Get an expression which might have comma separated expressions (as a BinaryExpression ,)
        /// </summary>
        /// <returns></returns>
        private IExpression Comma()
        {
            IExpression left = Assignment();

            while (Match(TokenType.COMMA))
            {
                // Get the previous comma as an operator
                Token op = Previous();
                IExpression right = Assignment();
                left = new BinaryExpression(left, op, right);
            }

            return left;
        }

        /// <summary>
        /// Get an expression which might contain a variable assignment (IDENTIFIER = VALUE)
        /// </summary>
        private IExpression Assignment()
        {
            IExpression expression = Or();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                IExpression value = Assignment();

                if (expression is VariableExpression)
                {
                    Token name = ((VariableExpression)expression).name;
                    return new AssignExpression(name, value);
                }

                // Error but don't throw because we don't need to resynchronize, not confused state
                Error(equals, "Invalid assignment target.");
            }

            return expression;
        }

        /// <summary>
        /// Get an expression which might contain an OR condition.
        /// </summary>
        private IExpression Or()
        {
            IExpression expression = And();

            // Keep looping through all OR expressions (and others)
            while (Match(TokenType.OR))
            {
                Token op = Previous();
                IExpression right = And();
                expression = new LogicalExpression(expression, op, right);
            }

            return expression;
        }

        /// <summary>
        /// Get an expression which might contain an AND condition.
        /// </summary>
        private IExpression And()
        {
            IExpression expression = Ternary();

            while (Match(TokenType.AND))
            {
                Token op = Previous();
                IExpression right = Ternary();
                expression = new LogicalExpression(expression, op, right);
            }

            return expression;
        }

        /// <summary>
        /// Get an expression which might contain a ternary conditional expression (? :)
        /// </summary>
        private IExpression Ternary()
        {
            IExpression expression = Equality();

            if (Match(TokenType.QUESTION))
            {
                IExpression thenExpression = Expression();
                Consume(TokenType.COLON, "Expect ':' after conditional expression.");
                IExpression elseExpression = Ternary();
                expression = new TernaryExpression(expression, thenExpression, elseExpression);
            }

            return expression;
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

            return Call();
        }

        private IExpression Call()
        {
            // Get callee
            IExpression expression = Primary();

            // Loop multiple times to allow for fn()()()
            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expression = FinishCall(expression);
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        /// <summary>
        /// Get a CallExpression once a callee has been matched.
        /// </summary>
        /// <param name="callee">The callee (ie. function name)</param>
        private IExpression FinishCall(IExpression callee)
        {
            List<IExpression> arguments = new List<IExpression>();

            // If there are arguments
            if (Check(TokenType.RIGHT_PAREN) == false)
            {
                // Loop to get every argument
                do
                {
                    // At this point, just use an object to pass data
                    if (arguments.Count >= 8)
                    {
                        // Don't throw because we know where we are (no panic mode)
                        Error(Peek(), "Function calls cannot have more than 8 arguments.");
                    }

                    // Match any expression after a Comma()
                    // Comma() has been implemented as a Binary(left , "," , right), so we need to skip over it for function calls
                    arguments.Add(Assignment());
                } while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new CallExpression(callee, paren, arguments);
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

            if (Match(TokenType.IDENTIFIER))
            {
                return new VariableExpression(Previous());
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
                    case TokenType.LET:
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