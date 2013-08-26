Front Language Compilation
--------------------------

  Once we have defined language by the set of the token classes (grammar),
we can create simple compiler which will transform source text to the
DLL which will directly call language class without lexer and parser.
It is possible because once parser desides to perform some action on
the token class, we can instead generate invocation code which will be called
later. Essentinally compiled code is just a sequence of posponed, serialized
invocations.
 Note that this is not real compilation because transformation to some target
language was not performed and compiled code has runtime dependency on the 
compiler DLL. So, this kind of compilation can be considered as a poor-mans 
solution which has runtime dependency penalty. While this solution works fine 
when .Net is a target language, it is not really usable when target language 
is JavaScript used on the customer web pages.
 To make difference from real language-to-language compiler, we will call this
kind of compiler a freezer or freeze-compiler.