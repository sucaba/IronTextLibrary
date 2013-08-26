Iron Text Library
=================

DSL and Programming Language implementation library for .Net

Current Version
---------------
0.9 

No releases by now!

Features
--------

Iron Text is a library for rapid creation of DSL and programming languages using c#
without additional external tools.

The distinguishing features of the project are that:
- grammar and lexical rules are described entirely using .net type system with custom attributes
- allows parsing any context-free grammar including ambiguous ones
- supports vocabularies of tokens and rules which can be reused in different languages
- generic methods can be used as a 'template rules'
- allows defining language abstraction using interfaces and abstract classes
  which can have multiple implementations for different parsing tasks.
- has built in error handling
- has built in line,column counting

Checkout samples on https://github.com/sucaba/IronTextLibrary

Technical Details
-----------------

Parser: LALR1, RNGLR
Scanner: RE2C like DFA compiled .net code
