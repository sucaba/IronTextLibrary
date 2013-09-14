What is Iron Text Library?
--------------------------

IronText is a DSL and Programming Language implementation library for .Net with
a remarkably low learning/maintenance threshold and at the same time powerful
enough to parse any context-free language.

Current Version
---------------

0.9.1.0 (1.0 beta)

NuGet Package
-------------

Consider installing IronText NuGet package directly from the Visual Studio.
See [IronText Package](http://www.nuget.org/packages/IronText/).

Features
--------

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

Why Yet-Another-Compiler-Compiler?
----------------------------------

There are so many parser, scanner generators and libraries around that it would
be difficult even to build a comprehensive list with all of them.
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

Samples
-------

See http://github.com/sucaba/IronTextLibrary/tree/master/Samples

Technical Details
-----------------

Parser: LALR1, RNGLR.  
Scanner: DFA compiled to a RE2C-like .net code.
