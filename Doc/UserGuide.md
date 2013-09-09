Iron Text Library User Guide
============================

Lexical and Syntax analysis stages
----------------------------------

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
and combines them using grammar rules into non-terminal tokens.

Parsing Process
---------------

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

Terminal and Non-Terminal Tokens
--------------------------------

  In IronText library terminal and non-terminal tokens are represented by types
and corresponding type instances. Additionally literal terminal tokens may be
referenced using constant strings.
  This representation approach allows smooth integration of parsing and OOP
concepts. As a result IronText library learning threshold is minimal for .Net
developers.

Defining Language
-----------------

Language is represented by any public interface or class marked with
LanguageAttribute.

Definition type is used as a definition for parse and scan rules and at the
same time it is used as a runtime instance for executing parse and scan
semantic action-methods. Methods and properties of a language-defining type may
(but not forced to!) represent lexical and syntactic rules. Additionally
language may reference language vocabularies (see below).

Example:
```csharp
[Language]
public interface IMyCommandLine
{
    ...
}
```


Defining Lexical Rules
----------------------

ScanAttribute and LiteralAttribute represent regular-expression and literal-matching
lexical rules correspondingly. Both attributes should be placed only on public methods.

ScanAttribute argument is a string with a scanner regular expression (SRE). SRE
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

LiteralAttribute can be replaced with a ScanAttribute holding
a single-quoted literal as follows:

```csharp
[Literal("foo")
FooTerm Foo();
```
can be replaced with:
```csharp
[Scan("'foo'")
FooTerm Foo();
```
However the are two significant advantages for using LiteralAttribute:
- When there is also one or more other Scan-rules which can match "foo" text.
  In this case literal-foo rule has priority over these scan-rules. In general,
  all Literal lexical rules have priority over the Scan lexical rules. This 
  way keywords can be distinguished from identifiers.
- Literal attribute also allows to reference literal token from ParseAttribute
  literal mask using literal text. See more details on ParseAttribute below.

If two Scan-rules match same text then the one which method happens first
in source code is used for terminal-token creation.  
Note: Relative precedence of Scan-rules associated with a same method cannot be
determined because of .Net framework limitations. But this issue can be
resolved by creating two methods.

If one Literal-rule is a prefix for another Literal-rule then scanner
priorities longer one.

Skip Scan Rules
---------------

Literal or ScanAttribute defined on a void method builds lexical rule which
does not produce any terminal tokens. These rules are called skip lexical rules.

Example:
```csharp
[Scan("'/*' (~'*'* | '*' ~'/')* '*/'")]
void MultiLineComment() { }

[Literal(",")]
void IgnoreComma() { }
```

It is illegal to reference skipped-literal in ParseAttribute literal-mask.


Parsing Rules
-------------

Parsing rules (aka syntax rules) can be defined using ParseAttribute.
Attribute should be placed only on public methods.

ParseAttribute defines grammar rule and at the same time uses target method as
a semantic action for this grammar rule. ParseAttribute can also accept
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

Additionally ParseAttribute can accept precedence and
associativity options [See Precedence].

Final Parse Rules
-----------------

In IronText, final parse rule can be defined by a ParseAttribute associated
with void-method. Since these rules produce "no result", there is nothing to
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
attribute at a glance), there is also ParseResultAttribute which is syntactic
sugar for the preceding code:

```csharp
[ParseResult]
List<Option> Options { get; set; }
```

There can be more than one final-rule and more than one parse-result property
in a language definition. In contrast, it is meaningless to define language
with no final parse rules.

Similarly to ParseAttribute, the ParseResultAttribute can accept literal-mask.
Example:
```csharp
// Options are surrownded by curly braces.
[ParseResult("{", null, "}")]
List<Option> Options { get; set; }
```

