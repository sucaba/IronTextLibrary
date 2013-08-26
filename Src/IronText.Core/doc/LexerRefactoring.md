Target "Token driven lexer": 
	delegate int RefillDelegate(char[] buffer)
	delegate IReciever<Msg> LexerDelegate(IReciever<Msg> tokenVisitor, RefillDelegate source);

1. TextSource which has buffer refilling functionality like in re2c. 
   Also performance tests are needed to build effective interface.
   void TextSource.RefillBuffer(char[] buffer);

   TextSource src = ...;
   char[] buffer = new char[1024];
   src.RefillBuffer(buffer);
   for (int i = 0; i != buffer.Lenght; ++i)
   {
		char ch = buffer[i];
		... DFA code ...
		... Refill ...
		... DFA code ...
   }

3. Standard Regex class is used only for SRE language bootstrapping.
4. SRE language and ILCompiler backend is used as a regex engine for lexer.
5. Compiled Lexer CIL (by the SRE backend) belongs to the language assembly (probably to the corresponding language class).
2. Factory methods belong to a language assembly (probably to the corresponding language class).

As a result there should be monolithic lexer CIL with scanning and token creation functionality.

Q/A:
1. Q: Should location information belong to the Stx or only to the Msg struct?
2. Q: How to ensure that Sym cannot be followed by another Sym (kind of word break functionality)?
3. Q: How to ensure that Sym pattern is matched in greedy mode ?

