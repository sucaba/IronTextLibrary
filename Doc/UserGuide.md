Iron Text Library User Guide
============================

Introduction
------------

### What is Iron Text Library? ###

### Why Yet-Another-Compiler-Compiler ? ###

There are so many parser, scanner generators and libraries around that it would
be difficult even to build a comprehensive list with all of them (you may look
at an attempt to build such list on
[Wikipedia](http://en.wikipedia.org/wiki/Comparison_of_parser_generators)).
So, why to introduce another tool and make this mess even larger?

In essence, the answer to this question is a _developer productivity_. 

IronText makes DSL and programming language development similar and highly
related to everyday coding activities. It benefits from the fact that there is
a strong semantic resemblance between abstract language specifications and
class, interface member definitions in languages like C# and Java. To get a
taste of this idea compare following simplified calculator language
specification in [EBNF
format](http://en.wikipedia.org/wiki/Backus%E2%80%93Naur_Form)

```ebnf
Result = Expr;                                                 
                                                 
Expr = Expr, "+", Expr;

Expr = Expr, "*", Expr;
                                                 
Expr = int;                                                 

int = digit, { digit };

digit = "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" | "0";
```

with a corresponding IronText definition:

```csharp                                
[Language]                                     
public interface ICalculator                                       
{                                                     
    [ParseResult]                                                
    Expr Result { get; set; }                                                 
                                                 
    [Parse(null, "+", null)]                                               
    Expr Plus(Expr x, Expr y);                                                
                                                 
    [Parse(null, "*", null)]                                               
    Expr Mult(Expr x, Expr y);
                                                 
    [Parse]                                              
    Expr Constant(int value);                                                 
                                                 
    [Scan("digit+")]                                                 
    int Number(string text);
}                                                
                                           
public abstract class Expr { }             
```
Note: Both EBNF-specification and IronText snippet are incomplete and are used
for demonstration purposes only. Missing elements are related to operator precedence
and associativity.

Also, in runtime, processing of the following calculator code:
```
2 * 3 + 7
```
will be equivalent to this c# code:

```csharp
ICalculator calc = ...; // parser or interpreter backend
calc.Result = 
    calc.Plus(
        calc.Mult(
           calc.Constant(calc.Number("2")),
           calc.Constant(calc.Number("3"))),
        calc.Constant(calc.Number("7")));
```

As you can see, IronText definition is comprehensive enough and, what is more important,
is native to a programming language in use.

### Existing Approaches, Tools and Libraries ###

Even though there is relatively not so many experts in parsing (and language
design in general) among software developers, a typical software product
contains at least command-line and configuration file text processing. This is
possible because such languages are relatively simple and are straightforward
to code by-hand. But what if you need to 

1. process more complex language 
2. start with a simple language and be able grow it
3. maintain it with a minimal extra cost
4. reuse language elements and logic in a family of languages, derive languages
   from existing ones
6. have the same approach for building simple and complex languages
   and still  
7. keep language creation and maintenance low enough to encorage using
   [Language-oriented
   programming](http://en.wikipedia.org/wiki/Language-oriented_programming) in
   your product.

Possible solution to 1 and 3 may be in using existing languages and
corresponding libraries. For instance, there are plenty of XML, JSON and HTML
libraries around. Another solution is to use programming language with powerful
enough metaprogramming features. This way metaprogramming constructs like
macros and monads can be used to create hosted version of your language.
Examples of such languages include:

- Lisp, Scheme, Clojure (macros)
- Haskell, F# (monads)

However these languages are either too complex to be adopted by an average
developer or have limitations making them not suitable for software development
industry.

In any case, both mentioned are partial solutions and are suitable only for a
limited subset of problems. So, what should one do to get more control over the
development of some language? 

The straightforward solution would be to use one of the modern parser
generators like an [ANTLR](http://antlr.org/). It is almost always possible to
find parser generator feature-rich enough for developing your DSL or
programming language. There are only two problems with this decision:

1. High learning threshold of a specification-language and a tool itself.
2. Adoption of the tool to a build process of your product.

While second is relatively easy to deal with, the first is typically bound to a
single experienced developer which is rather risky decision for big products.
So again, what to do if such problems are unwanted?

Another choice would be to use one on the parsing libraries. These allow
developer to specify and maintain language within a source code of the
programming language without external specifications.  
Practical examples of such libraries are:

- [Irony](http://irony.codeplex.com/) for .Net platform
- [Boost.Spirit](http://www.boost.org/libs/spirit) for C++
- [Simple Parser](http://cdsoft.fr/sp/sp.html) for Python

Most of these libraries have common implementation idea behind:

    Define syntactic and lexical elements using programming language
    expressions and improve readability by using operator overloading and
    metaprogramming (in c++) features of the hosting programming language.

This approach does not require external tools in product build process and
learning-maintenance threshold is lowered because of help from IDE and
compiler. However implementations of such libraries typically suffer from the 

1. parser algorithm limitations (LL, LALR1)
2. expression-specifications tend to be verbose, hence difficult to read

IronText also belongs to the category of parsing libraries and fixes these
issues.

First one is fixed by incorporating powerful [GLR][] algorithm along with
[LALR(1)][] (for simpler languages).  This way IronText allows to define any
context-free language, including ambiguous one.

[GLR]: http://en.wikipedia.org/wiki/GLR_parser
[LALR(1)]: http://en.wikipedia.org/wiki/LALR_parser

Second issue is fixed by using annotations (.Net attributes) instead of
expressions and operator overloading. Later allows to make language grammars
similar to a typical Object-Oriented API represented by interfaces and classes
and their members. With IronText approach developer can think of his language
specification as a Text API i.e. API which is invoked though the text.

IronText library is suitable for developing languages on the .Net framework
only.  However general idea is suitable also for JVM or any other platform with
support of reflection, annotations and VM-code generation.

### Features ###

- Syntax and lexical rules are described entirely using .net type system with
  custom attributes

- Supports any context-free language including ambiguous ones

- Supports vocabularies of lexical and syntax rules which can be reused in
  different languages

- Generic methods can be used as a 'template rules'

- Allows defining language abstraction using interfaces and abstract classes
  which can have multiple implementations for different parsing tasks.

- Language can be inherited from another without access to the source
  code of the base language.

- Language can be nested within other language with compatible lexical
  rules.

- Built-in error handling

- Built-in line, column counting

- Scanner supports *modes* to handle complex lexical elements like nested
  comments.

Basics
------

### Lexical and Syntax analysis stages ###

Context-free parsing consists of two stages:
- Lexical analysis (Scanner) produces terminal tokens.
In essence, scanner chops text into some basic elements which do not depend on
the current context of the input. Examples of such elements are 

    - delimiters: (, ), {, }, [, ]
    - numbers: 1234, 0xfa, 3.14
    - quoted strings: "hellow world"
    - keywords: if, while, begin, end
    - identifiers: myvar
    etc. 

    Some elements do not cause creation of terminal tokens, instead they
    are simply ignored. Most often these elments include:
    - space and tab characters
    - newlines
    - comments

- Syntax analysis (Parser) receives terminal tokens 
and combines them using syntax rules into non-terminal tokens.
All syntax rules of particular language form a *grammar*. This way
syntax rules may also be referenced as *grammar rules*.

### Parsing Process ###

As terminal tokens are delivered one-by-one to parser, the later tries to
combine them into meaningful constructs like expressions and statements. This
process is stopped successfully when scanner has no more text to process and
parser had applied one of the final syntactic rules along with associated
semantic action. In contrast, process fails when either is true:

- parser didn't receive enough input to combine it according to defined
  syntactic rules.
- parser received token which is not expected in a current context.
- scanner received text which cannot be processed according to the defined
  lexical rules.
- scanner stumbled upon the end-of-input in a middle of the current lexical
  element.

### Terminal and Non-Terminal Tokens ###

  In IronText library terminal and non-terminal tokens are represented by types
and corresponding type instances. Additionally literal terminal tokens may be
referenced using constant strings.
  This representation approach allows smooth integration of parsing and OOP
concepts. As a result IronText library learning threshold is minimal for .Net
developers.

Defining Language
-----------------

Language in Iron Text is represented by any public interface or class marked with
*LanguageAttribute*.

### Language Definition Type ###

Definition type is used as a definition for parse and scan rules and at the
same time it is used as a runtime instance for executing parse and scan
semantic action-methods. Methods and properties of a language-defining type may
(but not forced to!) represent lexical and syntactic rules. Additionally
language may reference language vocabularies (see below).

Example:
```csharp
[Language]
public interface IMyLanguage
{
    ...
}
```

### Defining Lexical Rules ###

*ScanAttribute* and *LiteralAttribute* represent regular-expression and literal-matching
lexical rules correspondingly. Both attributes should be placed only on public methods.

*ScanAttribute* argument is a string with a scanner regular expression (SRE). SRE
is designed to be readable inside C# string constants and is not compatible
with standard 'System.Text.RegularExpressions.Regex' class. For more SRE
details see below.

Example:
```csharp
[Language]
public class MyCommandLine
{
    public bool Recursive;
    public int  Level;

    ...

    [Scan("'--level=' digit+")] // "--level=" text followed by a one or more digits
    public LevelOption LevelOption(string text)
    { 
        Level = int.Parse(text.Substring("--level=".Length)); // Side effect for saving option value
        return null;      // Any values allowed
    }

    [Literal("--recurse")]
    public RecurseOption RecurseOption()
    { 
        Recursive = true; // Side effect for saving option value
        return null;      // Any values allowed
    }

    ...
}

// In MyCommandLine language RecurseOption represents terminal token.
// It can be returned only from lexical rules.
// Note: It is mistake to define also syntax (Parse) rules
//       which produce RecurseOption.
public interface RecurseOption { }
public interface LevelOption { }
```

It is possible to place multiple Scan and Literal attributes on the
same method. In this case each attribute will cause its own lexical
rule. 

*LiteralAttribute* can be replaced with a *ScanAttribute* holding
a single-quoted literal as follows:

```csharp
[Literal("foo")]
FooTerm Foo();
```
can be replaced with:
```csharp
[Scan("'foo'")]
FooTerm Foo();
```
However the are two significant advantages for using *LiteralAttribute*:
- When there is also one or more other Scan-rules which can match "foo" text.
  In this case literal-foo rule has priority over these scan-rules. In general,
  all Literal lexical rules have priority over the Scan lexical rules. This 
  way keywords can be distinguished from identifiers.
- Literal attribute also allows to reference literal token from *ParseAttribute*
  literal mask using literal text. See more details on *ParseAttribute* below.

If two Scan-rules match same text then the one which method happens first
in source code is used for terminal-token creation.  
Note: Relative precedence of Scan-rules associated with a same method cannot be
determined because of .Net framework limitations. But this issue can be
resolved by creating two methods.

If one Literal-rule is a prefix for another Literal-rule then scanner
priorities longer one.

### Skip Scan Rules ###

*Literal-* or *ScanAttribute* defined on a void method builds lexical rule which
does not produce any terminal tokens. These rules are called skip lexical rules.

Example:
```csharp
[Scan("'/*' (~'*'* | '*' ~'/')* '*/'")]
void MultiLineComment() { }

[Literal(",")]
void IgnoreComma() { }
```

It is illegal to reference skipped-literal in *ParseAttribute* literal-mask.


### Parsing Rules ###

Parsing rules (aka syntax rules) can be defined using *ParseAttribute*.
Attribute should be placed only on public methods.

*ParseAttribute* defines grammar rule and at the same time uses target method as
a semantic action for this grammar rule. *ParseAttribute* can also accept
literal-mask when one or more grammar rule tokens are literals (fixed text
tokens).  Literal-mask can contain nulls which correspond to method arguments
in left-to-right order. Trailing nulls in literal mask are optional.

Example:
```csharp
[Language]
public interface IMyScript
{
    ...
    
    // Syntax rule for if-statement
    [Parse("if", null, "then", null, "else", null)]
    IfStmt IfStmt(Expr cond, Stmt t, Stmt e);

    ...
}
```

Literals used in literal-mask don't need to have Literal-rules. They
can be defined either implicitly or explicitly.

Additionally *ParseAttribute* can accept precedence and
associativity options [See Precedence].

### Final Parse Rules ###

In IronText, final parse rule can be defined by a *ParseAttribute* associated
with a void-method. Since these rules produce "no result", there is nothing to
continue with and parsing process stops.

Example:
```csharp
[Language]
public interface IMyCommandLine
{
    [Parse]
    void AllOptions(List<Option> options);
}
```

In most cases, parser consumer also wants to have some sort of access to a
final parsing result. Because .Net property setters are also methods returning
'void', following is a good coding pattern for such situation:

```csharp
List<Option> Options { get; [Parse] set; }
```

This code works two-ways. First, it defines final-parse rule using setter
method. Second, it gives parser consumer access to the parsing result through
the property getter.

Because this coding pattern is a bit difficult to read (it is difficult to find
attribute at a glance), there is also *ParseResultAttribute* which is syntactic
sugar for the preceding code:

```csharp
[ParseResult]
List<Option> Options { get; set; }
```

There can be more than one final-rule and more than one parse-result property
in a language definition. In contrast, it is meaningless to define language
with no final parse rules.

Similarly to *ParseAttribute*, the *ParseResultAttribute* can accept literal-mask.
Example:
```csharp
// Options are surrownded by curly braces.
[ParseResult("{", null, "}")]
List<Option> Options { get; set; }
```

