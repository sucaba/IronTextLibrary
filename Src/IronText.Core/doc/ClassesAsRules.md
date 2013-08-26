Types as Tokens
---------------

.Net types can represent terminal and non-terminal symbols of the BNF grammar.
Correspondingly methods can be used to represent derivation rules for the
non-term class-tokens.

Terminal Tokens
----------------

Syntax Terms - instances of classes marked explicitly by TerminalAttribute

Programmatic Tokens
-------------------

This token type kind has no syntax structure at all. Instances are constructed and passesed programmatically.
From grammar point of view programmatic tokens are terminal tokens but have representation in lexer.

Non-Term Tokens
---------------

Non-term tokens can be of the following two types:
1. Internally structured - structure is defined by rules inside token class.
However structure can be exteneded by rules defined in participating language modules.
2. Externally structured. Any type can have structure defined in language module.

Language Module Classes
-----------------------

Language module classes contain rule methods.
Instance methods use module-class instance as a parsing context.

TODO: 
1. Get rid of internally-structured tokens in favour of language module classes.
2. There is no need in inheritance 

Reflection Layer
----------------

LanguageDescriptor:
	- Main Module

LanguageModuleDescriptor:
	- Referenced Modules
	- UsedTokenDescriptors
	- RuleDescriptors

TokenDescriptor
	: StxDescriptor                // Terminal (syntax) token descriptor
	| AnyTokenDescriptor           // Enables token inheritance
	| ExternalTokenDescriptor      // Used for token types embedding its own push-parsing state machine 
	| KeywordPlaceholderDescriptor // Temporary descriptor to represent not-yet-defined keyword token types
	| ListTokenDescriptor          // Token for zero-or-more sequences (TODO: move to language module)
	| CtxTokenDescriptor           // Fake token for parsing context argument type (TODO: remove after language module implementation)

