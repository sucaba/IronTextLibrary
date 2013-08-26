Pull logic and state machines
-----------------------------
- Instead of pull logic in parser and comipilers, use pull logic based on state machines.

Strategy
--------

"Make it fun! Start from simple things. Go towards complexity smoothly. Try to avoid complexity. Read theory to find ideas."

Plan
----
0. Reader
    - Syntax algebra/AST obects like in Scheme. 
      Syntax objects represent text with source information: file/url, line, column, position, span
    - Syntax processing language for compiling input syntax to VM instructions.
       - scope
       - namespaces
       - AST grammar types
1. Interpreter
    - core language
        - FFI & mapping to to .Net interfaces
			- invoke by name (MS DLR)
        - some builtins (read/write etc.)
        - <send> operation and Agent (Join calculus only)
        - <in> operation
        - Channel types: queued, constant, variable
        - Agents should be prototype based objects
        - Visitors
        - Collections
		- GC?
		- REPL
    - Async macro system and implicit phasing
    - Contexts
	- Debugger
3. Libraries I
	- S-templates
	- Pattern matching
	- Algorithms / Collections
2. Compiler
    - Javascript
    - .Net
	- Abstract VM & interpreter refactoring to reduce count of entities and simplify design.
    ~ ARM (mobile devices)
    ~ AMD32
4. Libraries II
	- Async tree. Idea is to organize *all* data as a tree with async protocol access. 
	  To represent graph *reference* nodes will be used.
	- HTTP Client - Server
	- AJAX based UI
	- Database interface
	- Distributed processing


S-expression simplifications
----------------------------

Scope idea
----------

Axioms:

1. S-EXPRESSION TRANSFORMATIONS. Any compilation, interpretation activity can
   be expressed as a sequence of transformations of input s-expression syntax
   to the output s-expression syntax.

2. TARGET PLATFORM. Final s-expression syntax should be semantically identical
   to the language of the target platform. Compilation from the final
   s-expression syntax to the target platform language like exe, javascript,
   HTML, XML should be built-in on the Wasp bootstrapping level and finally
   expressed in Wasp as bytecode and textual platform languages support.

3. INPUT AND OUTPUT REPRESENTATION. Both input and output are lists of syntax
   objects.

4. SINGLE SYNTAX OBJECT. In case when input or output needs to be single syntax
   object (identifier etc.) axiom #1 covers this scenario by implementing
   "single syntax object" list constraint on processing level.

5. SCOPE. Processing of input s-expressions is performed in context of object called *scope*.
   *Scope* is responsible for converting input list to output list according to 
   some internal rules. *Scope* can be also associated with a *language* of the 
   inner syntax.

6. SCOPE IMPLEMENTATION. To simplify processing performed by *scope* there
   should be some common ideas on the structure of the *scope* which will
   express common language processing ideas like bindings, macros, compilation
   error situations.

6.1. FLOATING SCOPE. One of ideas is *floating scope* which basically is a way to implement
   *scope*. Following rules express the main idea:

6.1.1. BUILTIN SCOPE. Initialy processing starts with a *floating scope*
   representing predefined *builtins* of the language.

6.1.2. PROCESSING. Processing consists with the sequence of processing steps on the
   list of the input syntax objects. Each step as an input has current 
   list of syntax objects and current *floating scope*. Each step produces
   new list of syntax objects and new *floating scope*.

6.1.2. END OF PROCESSING. Processing continues until the list of input syntax
   objects is empty.  At the end parent final *floating scope* peforms 
   *close step*. Result of close step is described in nested list processing.

6.1.3. RESULT OF PROCESSING. The result of processing list of input syntax objects
   is list of output syntax objects.

6.1.4. PROCESSING LEXEMES. Processing lexemes is performed according to the
   current *floating scope* rules. These rules allow matching particular
   lexeme types and perform builtin action on input and produce output.

6.1.5. PROCESSING LISTS.
   Nested input lists can be used to represent instructions, conditional
   constructs etc.
   Example:
    (module main)
    (print "hello world")
    (if (= x 4)
        (print "yes")
        (print "no"))
        
6.1.6. USE CASES.
   String lexeme can be used as regular runtime strings and mostly
   doesn't not change *scope* for the next processed instruction.
   Symbol lexemes starting with letters can be used to represent variable and
   function names.
   Symbol lexemes having digit number structure can represent numbers.   

6.2. Typical scope changes.
   Normally *scope* is changed in a limited number of 
   ways which gives a chance to make VM for syntax transformations.

6.2.1. CONSTANT SYNTAX.
   When currently processed syntax object is interpreted as a
   constant, *scope* doesn't need to be changed.
   Current *fscope* should be used to translate this 
   constant representation into the target language representation.

6.2.2. VALUE REFERENCE SYNTAX.
   When currently processed syntax object is interpreted as a
   reference to some value, *scope* doesn't need to be changed.
   Current *scope* should be used to translate reference
   to the value reference or constant in the target language.

6.2.3. KEYWORD SYNTAX.
   Lexeme matching particular "constant" format can be used as
   keyword to change *scope*.

6.2.4. INSTRUCTION SYNTAX.
   Instruction syntax is a non-empty list syntax object whos first
   syntax object is reference to the saved *scope*
   and the rest of list is processed using this *floating scope*.

6.2.5. DEFINITION SYNTAX.
   Definition syntax extends current *scope* with 
   new saved *scope* which can be referenced in
   subsequent syntax objects.
   Definition syntax is also *instruction syntax*.

6.2.6. INSTRUCTION AND VALUE REFERENCE SYNTAXES PROCESSING.
   Processing of syntaxes using bindings in current *floating
   scope* context can be done in following steps:
     - Get corresponding *scope* from current *scope*
     - Transfer control to this referenced *scope* by providing 
       additional flag determining whether identifying lexeme
       is on the begining of list or it is just reference.
       QUESTION: What is a good approach to provide this *flag* information???

6.2.7. PROCESSING DEFINITION SYNTAX.
   Special predefined *definition syntax* can 
   be used to define entire language in s-expression syntax.


---------------------------------

1. Calculator

Grammar:

(scope VM : external)

(scope Expr
...
  [(@+ : rest)
   (VM (push system+))
   (this : rest)]
  [(@* : rest)
   (VM (push system*))
   (this : rest)]
  [((@@number n) : rest)
   (VM (push n))
   :
   (this : rest)]
  [((f x y) : rest)
   (this f x y)
   (VM (call))
   (this : rest)]
   ...
   )

Sample:

(+ (* 3 (+ 1 4)) 9)

2. Adding variables

Grammar:

(scope VM external)

(scope Expr
...
  [((%lexeme n : NUMBER) : rest)
   (VM (push n))
   (this : rest)]
  [((@+ x y) : rest)
   (this @+ x y)
   (VM call)
   (this : rest)]
  [((@let var value) : rest)
   (scope (<new-scope> : this)
    [(@var : rest2)
     (this value : rest2)])
   (<new-scope> : rest)]
...
  )

Sample:

(let x 4)
(let y (+ x 100))
(* x y)

3. Scope blocks

Grammar:

(scope VM external)

(scope Expr
...
  [((@begin : expressions) : rest)
   (this : expressions)
   (this : rest)]
...
   )

Sample:

(let x 5)
(begin
  (let x 4) ; other x
  (let y (+ x 100)) ; using other x
  (* x y))
(+ x 2) ; use of first x

4. Lambda with single argument

Grammar:

(scope VM external)

(scope Expr
...
  [((@lambda arg : body) : rest)
   (scope (<body-scope> : this)
    [(@arg : rest2)
     (VM push-local 0)
     (this  : rest2)])
   (VM (procedure-start))
   (VM (pop-local 0))
   (<body-scope> : body)
   (VM (ret))
   (this : rest)]
...
   )

Sample:

(let x 5)
(begin
  (let x 4) ; other x
  (let y (+ x 100)) ; using other x
  (* x y))
(+ x 2) ; use of first x

5. Recursive let (single variable)

Grammar:

(scope VM external)

(scope Expr
...
  [((@let-rec var value) : rest)
   (scope (<new-scope> : this)
    [(@var : rest2)
     (<new-scope> value : rest2)])
   (<new-scope> : rest)]
...
  )

Sample:

(let-rec f
    (lambda (n)
        (if (= n 0) 
            1 
            (* n (f (- n 1))))))
(f 120)

7. Possibility to extend existing scope.
TODO

Constraint & pattern matching samples
-------------------------------------

(scope (Identifier))
(scope (String length))
(scope (Number))
(scope (Pair first rest))
    
((name) : body) => (define (name) : body)
====>
(scope Scheme
  (and
    (is Pair)
    (for first (is @lambda))
    (for rest     
      (is Pair)
      (for first        
        (is Pair)             
        (for first (is Identifier) name)
        (for rest (is Null)))
      (for rest (is Pair) body))
    (emit 
        (@define (name) 
          : body))))

    
8. Constraints VM Samples
-------------------------

;; Predefined patterns
- Identifier
- Number
- String
- Pair

Transforamtions (T):

(any S F)
==>
(do S)

(none S F)
==>
(do F)

(is S F <Pattern>)
==>
(match <Pattern> :Fail)
(diagnose:expect-not <Pattern>)
(do S)
(jump :End)
(label :Fail)
(diagnose:expect <Pattern>)
(do F)
(label :End)

9. Constraint VM commands
-------------------------
;; Extend current dictionary with new slots in case of success match of value in @this slot
(match pattern)

;; Push new dictionary with current dictionary as prototype and associates
;; @this slot with value previously stored in @name slot.
(push-this name)

;; Remove dictionary from the stack and set its prototype as current dictionary
(pop-this)

;; Copy value of @this slot to slot @name
(bind name)

;; Emits slot value to output stream
(emit name)

;; Emits constant value to output stream
(emit-const constant)

;; Open nested ouput stream
(open-stream)

;; close currently opened stream and restore previous stream
(close-stream)

;; Emit name as a *rest* value of stream.
;; Close currently opened stream and restore previous stream
(close-stream name)

pattern: 
    (NAME : NUMBER)
    |
    NAME
    |
    NUMBER


(push-match Pair ^name)
  (push-this first)
    (match Identifier ^fail)
    (pop-this)
    (push-this rest)
    (match Pair)
    (push-this first)
    (pop-this)
(pop rest)

^name: 

(push-match Identifier ^number)
    (open-stream)
    (emit-const IDENTIFIER)
    (close-stream this)
(pop-match)

^number:

(push-match Identifier ^fail)
  (open-stream)
  (emit-const NUMBER)
  (close-stream this)
(pop-match)

^success:

^fail:
...

     

1.
(is <Pattern>)
==>
(match <Pattern>)


2.
(scope <Pattern>
  ... constraint ...)
==>


3.
(and C0 C1)
==>
...

4.
(or C0 C1)
==>
(c C0)














