1. Unicode charsets.
2. "save <slot>" -> save-b <slot>, save-e <slot>
	(or (: (match X "foo") "end")
	    (: (match Y "foo") "bar"))
3. Match -> save all winning thread slots to the instance properties
4. Greedy/non-greedy strategies:
   Q: What if there are multiple matched threads and input is at the end?
   A:
	  - greedy strategy: take thread which has longest match.
		Optimization: on each input when new matched thread is being added, purge all matched threads with shorter match.
	  - nongreedy strategy: take thread which has shortest match.
	    Optimization: first matched thread is a winning one. There is no need to traverse input after that.

   Q: What if there are multiple matched threads and input is not at the end?
   A: See prev. question. Need to consume input according to the current strategy.

   Q: How to apply greedy/nongreedy strategy locally?
   Example: Get tail digits from the identifier:
		(: (match X (*? alphanumeric))
		   (match Y (* digit)))
		input | running threads							                   | matched threads
		------------------------------------------------------------------------------------
			  | [(<X> X="",Y="")]								           | [(X="",Y="")] // no optimization
		a     | [(<X> X="a",Y="") (<Y> X="a",Y="")]						   | (X="a",Y="") // no optimization
		5     | [(<X> X="a5",Y="")(<Y> X="a5",Y="") (<Y> X="a",Y="5")]     | (X="a",Y="") (X="a5",Y="") (X="a",Y="5")
		7     | [(<X> X="a57",Y="")(<Y> X="a57",Y="")(<Y> X="a5",Y="7") (<Y> X="a",Y="57")]     | (X="a",Y="") (X="a5",Y="") (X="a",Y="5") (X="a57",Y="")(X="a5",Y="7") (X="a",Y="57")
		EOI   | [] | 
		Deleted because of outer greedy: (l=1(-1,0), X="a",Y="") (l=2(-2,0), X="a5",Y="") (l=2(-1,1), X="a",Y="5") 
		Deleted because of the inner non-greedy/greedy: (l=3(-3,0): X="a57",Y="")(l=3(-2,1): X="a5",Y="7") 
		Success: (l=3(-1,2): X="a",Y="57")
	Analysis:
		101(1,100)
		101(100,0,1)
		Nodes: 
			score((greedy node))	=> matchlen(node) , score(node)
			score(nongreedy node)   => -matchlen(node), score(node)
			score(: node...)        => score(node), ...
			score(or node...)       => [score(node), ...]
			score(* node)           => 



    Q: How to optimize greedy/non-greedy algorithms:
    A: Because greedy/non-greedy strategy is used to analyze matches from the left to right, it is possible to
       throw away some threads which have lower "score" even if this score is partially calculated.

4. Integration with .Net code (SRE class instance, properties, methods).
	- when to set values to properties of instance i.e. how to resolve ambiguities?
		(or #\a #\b) 
	- greedy/nongreedy
	- search/match
    - STM implementation for slots?

5. Use SRE->IL compiler in generated language Lexers.
   Note: SRE language will be generated using bootstraping based on the .Net Regex class.

Possibilities/Questions:
1. DFA or cached DFA table in SRE->IL compiler.


Performance (relative to the native .Net Regex class):
------------------------------------------------------

1. string => int[] converstion adds 100% 
2. Nfa with cached Dfa is 33% faster than Regex, but 
   ILCompiler based (pure NFA) is ~4 times slower.
3. Code generation possibilities:
	NFA -> Code (current)
	DFA -> Code (how to generate DFA for regexps with special instructions/grouping?)
	NFA with cached DFA -> Code (possible?)
