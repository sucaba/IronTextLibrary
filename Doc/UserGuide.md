Iron Text Library v1.0. User Guide
==================================

Introduction
------------

### What is Iron Text Library? ###

IronText is a DSL and Programming Language implementation library for .Net with
a remarkably low learning/maintenance threshold and at the same time powerful
enough to parse any context-free language.

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
```cs
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

```cs
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
2. start with a simple language and be able grow it witout limitations
3. maintain it with a minimal extra cost
4. reuse language elements and logic in a family of languages
5. derive languages from existing ones  
and still  
6. keep language creation and maintenance low enough to encorage using
   [Language-oriented programming][LOP] in your product.

[LOP]: http://en.wikipedia.org/wiki/Language-oriented_programming

Possible solution to 1 and 3 may be in using existing languages and
corresponding libraries. For instance, there are plenty of XML, JSON and HTML
libraries around. Another solution is to use programming language with powerful
enough metaprogramming features. This way metaprogramming constructs like
macros and monads can be used to create hosted version of your language.
Examples of such powerful programming languages include:

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

Another choice would be to use one of the parsing libraries. These allow
developer to specify and maintain language within a source code of a
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
2. verobse, hence difficult to read expression-specifications

IronText also belongs to the category of parsing libraries and fixes these
issues.

First one is fixed by incorporating powerful [GLR][] algorithm along with
[LALR(1)][] (for simpler languages).  This way IronText allows to define any
context-free language, including ambiguous ones.

Second issue is fixed by using annotations (.Net attributes) instead of
expressions and operator overloading. This approach allows makes language
grammars code similar to a typical Object-Oriented API represented by
interfaces, classes and their members. With IronText approach developer can
think of his language specification as a Text API i.e. API which is invoked
though the input text.

IronText library is suitable for developing languages on the .Net framework
only.  However general idea is suitable also for JVM or any other platform with
support of reflection, annotations and VM-code generation.

### Features ###

- Syntax and lexical rules are described entirely using .Net type system with
  custom attributes

- Supports any context-free language including languages defined by ambiguous grammars

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
    - quoted strings: "hello world"
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


### Language Definition Type ###

Language in Iron Text is represented by any public interface or class marked
with *LanguageAttribute*. 

Definition type is used as a definition for parse and scan rules and at the
same time it is used as a runtime instance for executing parse and scan
semantic action-methods. Methods and properties of a language-defining type may
(but not forced to!) represent lexical and syntactic rules. Additionally
language may reference language vocabularies (see below).

Example:
```cs
[Language]
public interface IMyLanguage
{
    ...
}
```

### Using Language ###

Example of parsing string content:
```cs
IMyLanguage context = ...; // backend
Language.Parse(context, "2+2"); // Throws on a first error, ignores warnings and messages
```

Example of parsing from a TextReader:
```cs
IMyLanguage context = ...; // backend
string path = "Example1.calc";
Language.Parse(context, new StreamReader(path), path);
```

Example of parsing string content and show error messages in console:
```cs
IMyLanguage context = ...; // backend
using (var interp = new Interpreter<IMyLanguage>(context))
{
    interp.LogKind = LoggingKind.ConsoleOut;
    if (interp.Parse("2+2"))
    {
        Console.WriteLine("Success!");
    }
    else
    {
        Console.WriteLine(
            "====== Errors: {0}, Warnings: {1} ======",
            interp.ErrorCount,
            interp.WarningCount);
    }
}
```

Example demostrates more advanced Interpreter class functionality:  
- Parsing from a TextReader
- Redirect error messages to console
- Build parse tree. Actually it not a tree but a more powerful construct called
  Shared Packed Parse Forest (SPPF). It supports node sharing and ambiguous
  productions.
- Visit parse tree
- Tokenize from TextReader

```cs
IMyLanguage context = ...; // backend
string path = "Example1.calc";
using (var interp = new Interpreter<IMyLanguage>(context))
{
    interp.LogKind = LoggingKind.ConsoleOut;
    interp.Parse(new StreamReader(path), path);

    SppfNode tree = interp.BuildTree(new StreamReader(path), path);
    tree.Accept(new MySPPFVisitor());

    foreach (Msg token in interp.Scan(new StreamReader(path), path))
    {
        Console.WriteLine(token.Value);
    }
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
```cs
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

```cs
[Literal("foo")]
FooTerm Foo();
```
can be replaced with:
```cs
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

*Literal-* or *ScanAttribute* defined on a void method builds a lexical rule which
does not produce any terminal tokens. These rules are called skip lexical rules.

Example:
```cs
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
```cs
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

### Template Parsing Rules ###

TODO:

### Final Parse Rules ###

In IronText, final parse rule can be defined by a *ParseAttribute* associated
with a void-method. Since these rules produce "no result", there is nothing to
continue with and parsing process stops.

Example:
```cs
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

```cs
List<Option> Options { get; [Parse] set; }
```

This code works two-ways. First, it defines final-parse rule using setter
method. Second, it gives parser consumer access to the parsing result through
the property getter.

Because this coding pattern is a bit difficult to read (it is difficult to find
attribute at a glance), there is also *ParseResultAttribute* which is syntactic
sugar for the preceding code:

```cs
[ParseResult]
List<Option> Options { get; set; }
```

There can be more than one final-rule and more than one parse-result property
in a language definition. In contrast, it is meaningless to define language
with no final parse rules.

Similarly to *ParseAttribute*, the *ParseResultAttribute* can accept literal-mask.
Example:
```cs
// Options are surrownded by curly braces.
[ParseResult("{", null, "}")]
List<Option> Options { get; set; }
```

Parsing Non-Deterministic Languages
-----------------------------------

In formal grammar theory, the deterministic context-free grammars ([DCFG][]s) are a
proper subset of the context-free grammars. They are the subset of context-free
grammars that can be derived from deterministic pushdown automata, and they
generate the deterministic context-free languages. 

In context of IronText capabilities we will narrow definition of deterministic
grammar to a [LALR(1)][] grammars. All such grammars can be handled by the IronText
LALR1-automata.

In computer science, an [ambiguous grammar][AG] is a formal grammar for which
there exists a string that can have more than one leftmost derivation, while an
unambiguous grammar is a formal grammar for which every valid string has a
unique leftmost derivation. 

All other grammars are treated by IronText as non-deterministic grammars.

### Nondeterministic and Ambiguous Grammars ###

TODO:

### Semantic Actions Approach: Functional vs Side-Effects ###

TODO:

### Merging Ambiguities ###

Methods annotated with *MergeAttribute* can help in solving syntactic
ambiguities in ambiguous grammars. Once parser finds  a second way to combine
the same sequence of tokens, it will call merge method with 2 arguments. First
argument corresponds to the old non-token value and second is a new value of
this token. Because reductions in GLR parser always happen before shifts, you
can think of the first as a reduce choice and about second a shift choice.


Example of resolving operator precedence in runtime:
```cs
[Language]
public class AmbiguousCalculator
{
    [ParseResult]
    public Expr Result { get; set; }

    [Parse(null, "+", null)]
    public Expr Plus(Expr x, Expr y)
    {
        return new Expr(x + y, precedence: 1, accoc: Associativity.Left);
    }

    [Parse(null, "/", null)]
    public Expr Div(Expr x, Expr y)
    {
        return new Expr(x / y, precedence: 2, accoc: Associativity.Left);
    }

    [Parse]
    public Expr Constant(double value)
    {
        return new Expr(value, 10, Associativity.Left);
    }

    // Resolve operator precedence problems in runtime
    [Merge]
    public Expr MergeExpr(Expr reduceFirst, Expr shiftFirst)
    {
        // High precedence rule should be reduced first: left child in parse tree
        // and low precedence rule should be reduced last: parent in parse tree.
        // Merge method should return tree with a lowest precedence.
        if (reduceFirst.Precedence < shiftFirst.Precedence)
        {
            return reduceFirst;
        }
        if (reduceFirst.Precedence > shiftFirst.Precedence)
        {
            return shiftFirst;
        }

        switch (reduceFirst.Associativity)
        {
            case Associativity.Left: return reduceFirst;
            case Associativity.Right: return shiftFirst;
            default:
                throw new InvalidOperationException("Unable to resolve ambiguity.");
        }
    }
    
    [Scan("digit+ ('.' digit*)? | '.' digit+")]
    public double Real(string text)
    {
        return double.Parse(text);
    }
}

public class Expr
{
    public int           Precedence;
    public double        Value;
    public Associativity Associativity;

    public Expr(
        double        value,
        int           precedence,
        Associativity assoc)
    {
        ...
    }
}
```

Using this language, input: `2+8/2` can be interpreted in two ways:  
- `(2 + 8) / 2` - precedence of `/` is 2, topmost parse-tree node has higher precedence
- `2 + (8 / 2)` - precedence of `+` is 1, topmost parse-tree node has lower precedence


Advanced Features
-----------------

### Language Services ###

Language services are services which framework can assign to the properties of
a language type annotated using *LanguageService* attribute. There are only
three services:
1. IScanning - available from a scan-rule actions.
2. IParsing - available from a parse-rule actions.
3. ILogging - available during entire parsing process.

Example:
```cs
[Language]
public class MyLang
{
    [LanguageService]
    public IScanning Scanning { get; set; }

    [LanguageService]
    public IParsing Parsing { get; set; }

    [LanguageService]
    public ILogging Logging { get; set; }

    ...

    [Parse("when", null, null)]
    [Obsolete]
    public WhenExpr When(Expr cond, Stmt stmt)
    {
        Logging.Write(
            new LogEntry
            {
                Severity  = Severity.Error,
                HLocation = Parsing.HLocation,
                Message   = "When expression is obsolete. You should rewrite it using if-statement.",
            });

        return new StubWhenExpr();
    }

    ...

    [Scan("',' | '.'")]
    public Delimiter Delimiters(string text)
    {
        if (text == ",")
        {
            // Emit warning
            Logging.Write(
                new LogEntry
                {
                    Severity  = Severity.Warning,
                    HLocation = Scanning.HLocation,
                    Message   = "Commas are obsolete.",
                });

            // Token will not be delivered to a parser.
            // All commas will be ignored.
            Scanning.Skip();
        }

        return null;
    }

    ...
}

public interface Delimiter { }
```

### Vocabularies and Contexts ###

*Vocabulary* is a container for syntax and lexical elements similar to one
defined by the *LanguageAttribute*. Difference is that vocabulary does not
cause language creation, hence cannot be used directly for parsing. The only
purpose of *vocabulary* is to hold elements which can be reused in other
languages or vocabularies.

Vocabulary is defined using *VocabularyAttribute* with a public interface or
class. Vocabulary can be used in other language or vocabulary by using
*SubContextAttribute* annotating public property.

Example:
```cs
// Vocabulary responsible for variable
// definition and reference.
[Vocabulary]
public interface IVariableScope
{
    // Variable reference
    [Parse]
    VarRef Ref(string variableName);

    // Variable definition
    [Parse]
    VarDef Def(string variableName);
}

[Language]
public interface MyScript
{
    ...

    // Should return non-null instance!
    [SubContext]
    IVariableScope Scope { get; }

    [Parse]
    Expr VariableRefExpr(VarRef ref);

    [Parse(null, "=", null)]
    Stmt Assignment(VarDef def, Expr rhe);

    ...
}
```

It is also possible to define vocabulary using static-class and use it by
annotating destination language or vocabulary with *StaticContextAttribute*.
Static contexts may be useful for defining general purpose elements and
primitive elements of language which do not depend on the context.

Example:
```cs

// Defines template-rule for parsing list of any items.
[Vocabulary]
public static class Collections
{
    [Parse]
    public static List<T> Empty<T>() 
    { 
        return new List<T>();
    }

    [Parse]
    public static List<T> List<T>(List<T> items, T newItem)
    {
        items.Add(newItem);
        return items;
    }
}

// Parses list of integers
[Language]
[StaticContext(typeof(Collections))]
public class IntegerList
{
    [ParseResult]
    public List<int> Result { get; set; }

    [Scan("digit+")]
    public int Integer(string text) { return int.Parse(text); }

    [Scan("blank+")]
    public void Blank() {}
}
```

### Scanner Modes ###

Language which is able to parse nested comments without recursive parser rules:

```cs
[Language]
public class NestedCommentSyntax
{
    [ParseResult] 
    public string Result { get; set; }

    [LanguageService]
    public IScanning Scanning { get; set; }

    [Literal("/*")]
    public void BeginComment(out CommentMode mode)
    {
        mode = new CommentMode(this, Scanning);
    }
}

[Vocabulary]
public class CommentMode
{
    private StringBuilder comment;
    private readonly NestedCommentSyntax exit;
    private int nestLevel;
    private IScanning scanning;

    public CommentMode(NestedCommentSyntax exit, IScanning scanning)
    {
        this.scanning = scanning;
        this.exit = exit;
        this.comment = new StringBuilder();

        BeginComment();
    }

    [Literal("/*")]
    public void BeginComment()
    {
        ++nestLevel;
        Console.WriteLine("increased comment nest level to {0}", nestLevel);
        comment.Append("/*");
    }

    [Scan("(~[*/] | '*' ~'/' | '/' ~'*') +")]
    public void Text(string text)
    {
        comment.Append(text);
    }

    // Note: Typically comments are ignored in languages. In such case
    //       'EndComment()' method should return 'void' and 'scanning.Skip()'
    //       call is surplus.
    [Literal("*/")]
    public string EndComment(out NestedCommentSyntax mode)
    {
        comment.Append("*/");

        --nestLevel;
        Console.WriteLine("decreased comment nest level to {0}", nestLevel);
        if (nestLevel == 0)
        {
            mode = exit;
            return comment.ToString();
        }
        else
        {
            mode = null;            // null mode means don't change current mode.
            scanning.Skip();        // scanner should skip current token value 
            return null;            // Ignored token value
        }
    }
}
```

### Demands ###

### Local Contexts ###

### Metadata Extensibility and Reporting ###

### Default Context Implementation ###

When Interpeter instance is created without context instance, it will create
its own default context instance. 

When language definition type is interface, abstract class then IronText
compiler will generate a factory for default context instances along with a
classes implementing abstract types. All abstract methods will be implemented
as follows:  
- method returning reference-token will return nulls 
- method returning value-token will return default-value
- method returning demand-token will return default non-null instances of demand-token type.
- SubContext property will return non-null instance of vocabulary

When language definition type is not abstract then factory will contain simply
invocation of the default constructor. 

When default constructor is not available framework will report a build-time
error.

Example demonstrates Interpeter with default context implementation.
```cs
[Language]
public interface IMyLanguage
{
    ...
}

...

// Actual implementation for IMyLanguage is not specified
using (var interp = new Interpeter<IMyLanguage>())
{
    if (!interp.Parse("foo"))
    {
    }
}
```

Scanner Regular Expressions (SRE)
---------------------------------

SRE was designed to be readable enough inside c# strings
and at the same time comprehensive enough in generated
language documentation.

### Primitives ###

<table>
<tr>
    <td>.</td>                 <td>Any character</td>
</tr>
<tr>
    <td>'x'</td>               <td>Single character</td>
</tr>
<tr>
    <td>'hello \r\nworld'</td> <td>Quoted string</td>
</tr>
<tr>
    <td>[abcdef]</td>          <td>Character enumeration</td>
</tr>
<tr>
    <td>'0'..'9'</td>          <td>Character range</td>
</tr>
<tr>
    <td>u00C0</td>          <td>Unicode character</td>
</tr>
<tr>
    <td>U0010abcd</td>      <td>Unicode surrogate. Represented as a 2-character string</td>
</tr>
<tr>
    <td>Zs</td>             <td>Unicode case-sensitive category name</td>
</tr>
</table>

### Predefined Character Sets ###

<table>
<tr>
    <td>alnum</td> <td>ASCII alphabet and numbers</td>
</tr>
<tr>
    <td>alpha</td> <td>ASCII alphabet</td>
</tr>
<tr>
    <td>blank</td> <td>SPACE (0x32) and TAB (0x09) characters</td>
</tr>
<tr>
    <td>digit</td> <td>Decimal digits</td>
</tr>
<tr>
    <td>esc</td> <td>Backslash character '\'</td>
</tr>
<tr>
    <td>hex</td> <td>Hexadecimal digits: '0'..'9', 'a'-'f', 'A'-'F'</td>
</tr>
<tr>
    <td>print</td> <td>ASCII printable characters</td>
</tr>
<tr>
    <td>quot</td> <td>Double quote '"'</td>
</tr>
<tr>
    <td>zero</td> <td>Zero-character '\0'</td>
</tr>
</table>

### Operators ###

Operators sorted by ascending precedence:

<table>
<tr>
    <th>Operator</th> <th>Meaning</th>
</tr>
<tr>
    <td>... | ...</td> <td>alternative</td>
</tr>
<tr>
    <td>... ...</td> <td>sequence</td>
</tr>
<tr>
    <td>... *</td> <td>previous atom should appear zero or more times</td>
</tr>
<tr>
    <td>... +</td> <td>previous atom should appear one or more times</td>
</tr>
<tr>
    <td>... ?</td> <td>previous atom is optional</td>
</tr>
<tr>
    <td>... {N,M}</td> <td>repeat previous atom from N to M times inclusively</td>
</tr>
<tr>
    <td>... {N,}</td> <td>repeat previous atom at minimum N times</td>
</tr>
<tr>
    <td>... {N}</td> <td>repeat previous atom exactly N times</td>
</tr>
<tr>
    <td>( ... )</td> <td>grouped expressions</td>
</tr>
<tr>
    <td>~ ...</td> <td>complemented character set</td>
</tr>
</table>


### Full SRE Grammar ###

```
01: void -> Regexp
02: Regexp -> Branch
03: Branch -> 
04: Branch -> Piece[]
05: Piece[] -> Piece
06: Piece -> Piece '?'
07: Piece -> Piece '*'
08: Piece -> Piece '+'
09: Piece -> Piece '{' Integer '}'
10: Piece -> Piece '{' Integer ',' Integer '}'
11: Piece -> Piece '{' Integer ',' '}'
12: Piece -> '(' Regexp ')'
13: Piece -> 'action' '(' Integer ')'
14: Piece -> QStr
15: Piece -> IntSet
16: IntSet -> '~' IntSet
17: IntSet -> '~' '(' CompositeIntSet ')'
18: CompositeIntSet -> IntSet
19: CompositeIntSet -> CompositeIntSet '|' IntSet
20: IntSet -> Chr '..' Chr
21: IntSet -> Chr
22: IntSet -> CharEnumeration
23: IntSet -> 'alnum'
24: IntSet -> 'alpha'
25: IntSet -> 'blank'
26: IntSet -> 'digit'
27: IntSet -> 'esc'
28: IntSet -> 'hex'
29: IntSet -> 'print'
30: IntSet -> 'quot'
31: IntSet -> 'zero'
32: IntSet -> '.'
33: IntSet -> $id
34: Piece[] -> Piece Piece
35: Piece[] -> Piece Piece Piece
36: Piece[] -> Piece Piece Piece Piece List<Piece>
37: List<Piece> -> 
38: List<Piece> -> List<Piece> Piece
39: Regexp -> Regexp '|' Branch

'?' -> '?'
'*' -> '*'
'+' -> '+'
'{' -> '{'
'}' -> '}'
',' -> ','
'(' -> '('
')' -> ')'
'action' -> 'action'
'~' -> '~'
'|' -> '|'
'..' -> '..'
'alnum' -> 'alnum'
'alpha' -> 'alpha'
'blank' -> 'blank'
'digit' -> 'digit'
'esc' -> 'esc'
'hex' -> 'hex'
'print' -> 'print'
'quot' -> 'quot'
'zero' -> 'zero'
'.' -> '.'
Chr -> ['] (~['\\] | [\\] .) [']
QStr -> ['] ~['\\]* ( [\\] .  ~['\\]* )* [']
CharEnumeration -> '[' ~[\]\\]* ( [\\] . ~[\]\\]* )* ']'
Chr -> 'u' hex {4}
QStr -> 'U' hex {8}
$id -> alpha alnum+
Integer -> digit+
void -> '\r'? '\n' | u0085 | u2028 | u2029
void -> blank+
```

[GLR]:     http://en.wikipedia.org/wiki/GLR_parser
[LALR(1)]: http://en.wikipedia.org/wiki/LALR_parser
[DCFG]:    http://en.wikipedia.org/wiki/Deterministic_context-free_grammar
[AG]:      http://en.wikipedia.org/wiki/Ambiguous_grammar
