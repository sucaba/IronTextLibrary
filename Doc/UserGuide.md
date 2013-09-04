Iron Text Library User Guide
============================

Main concepts
-------------

1) Lexical and Syntax analysis stages.

Context-free parsing consists of two stages:
- Lexical analysis (Scanner) produces terminal tokens which internally 
are represented by Msg class.  It contains integer token identifier, value,
and location of the token in text.
- Syntax analysis (Parser) receives terminal tokens (sequence of Msg
instances) and combines them using grammar rules.

In essense, scanner chops text into some basic elements which do not depend on
the current context of the input. Examples of such elements are 
    - delimiters: (, ), {, }, [, ]
    - numbers: 1234, 0xfa, 3.14
    - quoted strings: "hellow world"
    - keywords: if, while, begin, end
    - identifiers: myvar
etc.

As these elements are delivered one-by-one to a parser, the later tries to
combine them into meaningful constructs like expressions and statments. This
process is stopped successfully when scanner has no more text to process and
parser had applied one of the final grammar rules along with associated
semantic action. In contrast, process fails when either 
    a. parser didn't receive enough input to combine it according to the
       syntactic rules.
    b. parser recieved token which is not expected in a current context.
    c. scanner recieved text which cannot be processed according to the
       defined lexical rules.
    d. scanner stumbled upon the end-of-input in the middle of the current
       lexical element.

2) ScanAttribute and LiteralAttribute.
Target: public methods.

ScanAttribute marks method which is used to create terminal token.
Attribute should have argument defining pattern for text matching.


3) LanguageAttribute.
Target: public interface or class.

Marks language definition type which is used as a source grammar for parse and
scan rules and at the same time is used as a runtime context for executing 
parse and scan semantic actions.

Ex:
```csharp
[Language]
public interface IMyCommandLine
{
    ...
}
```

4) ParseAttribute.
Target: public interface or class method.

Defines grammar rule and at the same time uses target method as a semantic
action for this rule. ParseAttribute can accept literal-mask when one or more
grammar rule tokens are literals (fixed text tokens).  Literal-mask can
contain nulls which correspond to method arguments in left-to-right order.
Trailing nulls in literal mask are optional.

Ex:

```csharp
[Language]
public interface IMyScript
{
    ...
    
    // Parsing rule for if-statement
    [Parse("if", null, "then", null, "else", null)]
    IfStmt IfStmt(Expr condition, Stmt thenStmt, Stmt elseStmt);

    ...
}
```

Additionally ParseAttribute can accept precedence and
associativity options [See Precedence].

5) ParseResultAttribute

Parsing ends when any of the final grammar rules are applied. Final rule is
typically associated with a setter of a *parser result* property marked with 
ParseResultAttribute.

Ex:

```csharp
[Language]
public interface IMyCommandLine
{
    [ParseResult]
    List<Option> Options { get; set; }
}
```

There can be more than one result property in language.

Since language is not required to produce meaningful result to user, it is
also possible to define final parse rule on a void-method using regular 
ParseAttribute.

Ex:

```csharp
[Language]
public interface IMyCommandLine
{
    [Parse]
    void AllOptions(List<Option> options);
}
```

In fact 

```csharp
[ParseResult]
List<Option> Options { get; set; }
```

is just syntactic sugar for:

```csharp
List<Option> Options { get; [Parse] set; }
```

i.e. void (in case of property a setter return type) is a start-token type of 
any IronText parser.

Similarly to the ParseAttribute, ParseResultAttribute can accept literal-mask.
Ex:
```csharp
// Options are surrownded by curly braces.
[ParseResult("{", null, "}")]
List<Option> Options { get; set; }
```

