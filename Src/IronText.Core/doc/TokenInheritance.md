Keyword as Identifier Problem
-----------------------------
  Some terminals can be treated differently in different situations.
Example:
Token IF can be treated as a keyword in one place and as a regular
identifier in another place. Even though it is dangerous to use keywords
as identifiers, it may be required by a language. The problem is that
we still want to keep grammar context-free such that lexer will not
depend on the parser state.
  Solution to this problem is relatively simple.
Lets assume that there are two BNF grammar rules which treat keyword IF differently:
	A) T -> ... IF ...   // as keyword
	B) T -> ... Idn ...  // Idn should also handle an IF keyword token
It is obvious that parser cannot be context free if we want to allow IF
keyword term on the place of the Idn term.
To solve problem we need to define new grammar rule:
    Rx -> Idn | IF 
And replace Idn token in rule B:
	B) T -> ... Rx ...   // now parse tables will handle IF correctly
This approach allows to include or exclude any keyword at the place
where symbol is expected.

Inhertiance Roles
-----------------

Base -> Child inherited token classes can mean one of the following:
1) Base is a token class or interface actually used in grammar, but Child is actual implementation
   of the Base and is provided by token factory in runtime. In this case abstract language is reperesented 
   by abstract token types and actual language processor uses real token types created by token factory. 
   (Same aplies to context types).
2) Child is independed token extended with methods-rules and OOP logic in Base. 
   Also rules can be overriden. (ExtensionAttribute)
3) Inheritance alternative. Base class represents a token which can be defined as a one or more derived classes.
   In this case derived classes are not extended with rules in base class to prevent reduce-reduce conflicts while
   referencing Base class. Also derived classes marked by ExtensionAttribute cannot act as inheritance alternatives.

Using Inheritance for Representing Alternate Rules
--------------------------------------------------
Inherited alternatives are all inherited Base -> Child type pairs
for which Base type is used in grammar explicitly and Child is either 
used explicitly in grammar or forcibly used via one of the following:
1) UseTokenAttribute  defined on any grammar token class.
2) UseAllInheritedAttribute defined on the Base.

Implicit Keyword Problem
------------------------
  Thies approach is simple when using bison/yacc or Wasp Pattern-rules,
however it will cause problem when keyword-type was generated implicitly
during the processing Operative-rule (method with OperativeAttribute).
  To solve this we need some implicit mechanism for using Idn itself
or Idn with keyword.
may cause some obstacles with Wasp Operative rules. Problem is that 
keywords are generated implicitly and coder has no possibility to 
provide keyword type to build rule Rx explicitly. To avoid we can use all inherited tokens
used in grammar.

So, we need to provide some built-in type
to change grammar mapping behavior.
  Because using keywords as identifiers in many cases is unsafe 
approach, we will use exact term matching in case of Idn and other
non-abstract classes and will use all inherited types for abstract
syntax parameter types. To force inheritance in case of non-abstract 
syntax types will use generic type "Any<>".
Our initial example with keyword and symbol may look like
naturally:

	public class T
	{
		// Automatically generated keyword type IF
		[Operative("IF")]
		public void R0(...) { ... }
		
		// Handle symbol by not including keywords
		[Pattern]
		public void R1(..., Idn sym, ...) { ... }
	}

Generated parser will treat "Idn" by not including keywords.
To include keywords, we will use Any<Idn> parameter type instead:

	public class T
	{
		// Automatically generated keyword type IF
		[Operative("IF")]
		public void R0(...) { ... }
		
		// Handle Idn including all inerited terminal 
		// types (both compile time and generated at runtime)
		[Pattern]
		public void R1(..., Any<Idn> sym, ...) { ... }
	}

Note that in later case action of the R1 rule can distinguish 
keyword types from regular Idn instances by the c# 
expression "sym is Kwd".

Non-terminal types inheritance
------------------------------
	
Token inheritance can be useful not only for terminal symbols
to avoid context-sensitive grammars, but also for non-terminals
to provide AST node hierarchy:

Example:
Following grammar types will result in a parser
for any s-expression and the outcome of this parser
will be corresponding AST tree.

	// Grammar start
    public class AstStart { [Pattern] public void R0(List<StxNode> topNodes) { } }

    [UseToken(typeof(IdnLeaf))]
    [UseToken(typeof(StrLeaf))]
    [UseToken(typeof(StxComposite))]
	public abstract class StxNode { }

	public class IdnLeaf : StxNode { [Pattern] public void R0(Idn idn) { } }

	public class StrLeaf : StxNode { [Pattern] public void R1(Str str) { } }

	public class StxComposite : StxNode
	{
		public List<StxNode> Children;
	
		[Pattern]
		public void Nested(Opn opn, List<StxNode> children, Cls cls)
		{
			Children = children;
		}
	}

Note that as in case of syntax classes (term classes), abstract class parameter
will automatically include entire hierarhy. And similary for non-abstract class parameters,
hierarhy will not be used untless type is wrapped by the Any<> generic type.

How to distinguish term types from non-term types?
--------------------------------------------------

  TerminalAttribute is needed only for lexer to get Regex information for terminal.
If one or more types passed to lexer constructor do not provide Regex information
then lexer should report error.

  Parser table generator should be able to distinguish terminal types from the non-
terminal types. This can be done in following ways:
	1) Default. Default approach for distinguishing terminal and non-terminal tokens
	is to check assignability to the Stx type.
	2) Provide Type predicate which will distinguish terminal type from the non-terminal type.
	3) Provide List of terminal types. Then GrammarMapping will skip registration of any non-term
	types present in this list.



Classification
--------------

Input:
1) Type.IsAbstract
2) Private/Internal/Public.
3) Has ImplicitUseAttribute
4) typeof(Stx).AssignambleFrom(Type)

Output:

???
