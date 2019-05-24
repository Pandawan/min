# min

A small project to teach myself creating interpreted programming languages.

I am following the [Crafting Interpreters book](https://craftinginterpreters.com/) by Bob Nystrom.

## Try it

With .NET Core 2.2 installed, simply run `dotnet run --project min` at the root of the repository to try it.

## Tools

If you want to use one of the tools, run `dotnet run --project tools <tool_name> [...args]` at the root of the repository.

Available tools:

- `gen_ast [expression|statement] <output_directory>`: Generates an `Expressions.cs` or `Statements.cs` file for Abstract Syntax Tree use.

## Additions

Alongside the features supported in Crafting Interpreters, I've added

- Ternary operator `? :`
- Comma operator `,`
- (Non-nested) block comments `/* */`

## TODO (after)

- Add advanced operators (modulo, exponent, bitwise)
- Add `==` vs `===` like in JavaScript
- Add Array support `[]`
- Add `instanceof` operator?
- Change `print` from a keyword to a function
- Add `foreach` loops (for in, for of, etc.)
- Use `:` for inheritance, not `<`
- Add a `Min.version` as part of the Min library/object (kind of meta object)
- Add nested block comment support `/* /* */ */`
- Add comparison support for strings `5 < "hey"`
