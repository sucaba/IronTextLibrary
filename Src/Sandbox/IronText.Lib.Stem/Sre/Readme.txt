The SRE regular-expression notation
Olin Shivers
August 1998

This document can be viewed in emacs outline mode, with *'s introducing
section headings -- just say M-x outline-mode in emacs.

-------------------------------------------------------------------------------
* Table of contents
-------------------
Preamble: 100% and 80% solutions
Overview
Summary SRE syntax
Examples
A short tutorial
Discussion and design notes
Regexp functions
The scsh regexp ADT 
Syntax-hacking tools
Acknowledgements

-------------------------------------------------------------------------------
* Preamble: 100% and 80% solutions
----------------------------------
There's a problem with tool design in the free software and academic
community.  The tool designers are usually people who are building tools for
some larger goal. For example, let's take the case of someone who wants to do
web hacking in Scheme.  His Scheme system doesn't have a sockets interface, so
he sits down and hacks one up for his particular Scheme implementation. Now,
socket API's are not what this programmer is interested in; he wants to get on
with things and hack the exciting stuff -- his real interest is Web services. 
So he does a quick 80% job, which is adequate to get him up and running, and
then he's on to his orignal goal.

Unfortunately, his quickly-built socket interface isn't general. It just
covers the bits this particular hacker needed for his applications. So the
next guy that comes along and needs a socket interface can't use this one.
Not only does it lack coverage, but the deep structure wasn't thought out well
enough to allow for quality extension.  So *he* does his *own* 80%
implementation. Five hackers later, five different, incompatible, ungeneral
implementations had been built. No one can use each others code.

The alternate way systems like this end up going over a cliff is that the
initial 80% system gets patched over and over again by subsequent hackers, and
what results is 80% bandaids and 20% structured code. When systems evolve
organically, it's unsuprising and unavoidable that what one ends up with is a
horrible design -- consider the DOS -> Win95 path.

As an alternative to five hackers doing five 80% solutions of the same
problem, we would be better off if each programmer picked a different task,
and really thought it through -- a 100% solution. Then each time a programmer
solved a problem, no one else would have to redo the effort. Of course, it's
true that 100% solutions are significantly harder to design and build than 80%
solutions. But they have one tremendous labor-savings advantage: you don't
have to constantly reinvent the wheel. The up-front investment buys you
forward progress; you aren't trapped endlessly reinventing the same awkward
wheel.

Examples: I've done this three times. The first time was when I needed an
emacs mode in graduate school for interacting with Scheme processes. I looked
around, and I found a snarled up mess of many, many 80% solutions, some for
Lisp, some for Scheme, some for shells, some for gdb, and so forth. These
modes had all started off at some point as the original emacs shell.el mode,
then had been hacked up, eventually drifting into divergence. The keybindings
had no commonality. Some modes recovered old commands with a "yank" type form,
on c-c y. Some modes recovered old commands with m-p and m-n. It was hugely
confusing and not very functional.

The right thing to do was to carefully implement one, common base mode for
process interaction, and to carefully put in hooks for customising this base
mode into language-specific modes -- lisp, shell, Scheme, etc. So that's what
I did.  I carefully went over the keybindings and functionality of all the
process modes I could find -- even going back to old Lisp Machine bindings for
Zwei -- and then I designed and implemented a base mode called comint.  Now,
all process modes are implemented on top of comint, and no one, ever, has to
re-implement this code. Users only have to learn one set of bindings for
the common functions. Features put into the common code are available for free
to all the derived modes. Extensions are done, not by doing a completely new
design, but *in terms of* the original system -- it may not be perfect, but
it's good enough to allow people to move on and do other things.

The second time was the design of the Scheme Unix API found in scsh. Most
Schemes have a couple of functions for changing directory, some minimal socket
hacking, and perhaps forking off a shell command with the system() C function.
But no one has done a complete job, and the functions are never compatible.
It was a classic 80%-solution disaster.  So I sat down to do a careful, 100%
job -- I wanted to cover everything in section 2 of the Unix man pages, in a
manner that was harmonious with the deep structures of the Scheme language. As
a design task, it was a tremendous amount of work, taking several years, and
multiple revisions. But now it's done. Scsh's socket code, for instance,
*completely* implements the socket API. My hope in doing all this was that
other people could profit from my investment. If you are building your own
Scheme system, *you* don't have to put in the time. You can just steal the
design. Or the code.

The regexp notation in this document represents a third attempt at this kind
of design. Looking back, I'm amazed at how much time I poured into the design,
not to mention the complete reference implementation. I sold myself on doing a
serious job with the philosophy of the 100% design -- the point is to save
other people the trouble. If the design is good enough, then instead of having
to do your own, you can steal mine and use the time saved... to do your own
100% design of something *else*, and fill in another gap.

I am not saying that these three designs of mine represent the last word on
the issues -- "100%" is really a bit of a misnomer, since no design is ever
truly 100%. I would prefer to think of them as sufficiently good that they at
least present low-water marks -- future systems, I'd hope, can at least build
upon these designs, hopefully *in terms of* these designs. You don't ever have
to do *worse* -- you can just steal the design. If you don't have a
significantly better idea, I'd encourage you to adopt the design for the
benefits of compatibility. If you *do* have an improvement, email me about it,
so we can fold it in to the core design and *everyone* can win -- and we can
also make your improvement part of the standard, so that people can use your
good idea and *still* be portable.

But here's what I'd really like: instead of tweaking regexps, you go do your
own 100% design or two. Because I'd like to use them. If everyone does just
one, then that's all anyone has to do.
    -Olin


-------------------------------------------------------------------------------
* Overview
----------
This document describes the regular-expression system used in scsh.
The system is composed of several pieces:

- An s-expression notation for writing down general regular expressions.
  In most systems, regexps are encoded as string literals. In scsh,
  they are written using s-expressions. This notation has several
  advantages over the traditional string-based notation; these advantages
  are discussed in a following section.

- An abstract data type (ADT) representation for regexp values.
  Traditional regular-expression systems compute regular expressions
  from run-time values using strings. This can be awkward. Scsh, instead,
  provides a separate data type for regexps, with a set of basic constructor
  and accessor functions; regular expressions can be dynamically computed
  and manipulated using these functions.

- Some tools that work on the regexp ADT: case-sensitve -> case-insensitive
  regexp transform, a regexp simplifier, and so forth.

- Parsers and unparsers that can convert between external representations
  and the regexp ADT. The supported external representations are
      + Posix strings
      + S-expression notation
  Being able to convert regexps to Posix strings allows implementations
  to implement regexp matching using standard Posix C-based engines.

- Macro support for the s-expression notation.
  The RX macro provides a new special form that allows you to embed regexps in 
  the s-expression notation within a Scheme program. The macro parses the
  regexp into the ADT, simplifies it, and translates it to a Posix strings,
  which can be used by a traditional C-based regexp engine.

- Matchers
  Spencer's Posix regexp engine is linked in to the runtime; the
  regexp code uses this engine to provide text matching.
  
The regexp language supported is a complete superset of Posix functionality,
providing:
    sequencing and choice (|)
    repetition (*, +, ?, {m,n})
    character clases and wildcard ([...],  .)
    beginning/end of string anchors
    beginning/end of line anchors
    beginning/end of word anchors
    case-sensitivity control
    submatch-marking (...)
It can all be implemented using a Posix regexp engine.

-------------------------------------------------------------------------------
* Summary SRE syntax
--------------------
Here is a summary of the syntax; the next section is a friendlier tutorial
introduction.

SRE ::=
    <string>			literal match -- Interpreted relative to
				the current case-sensitivity lexical context
				(default is case-sensitive).

    (<string>)			Set of chars, e.g., ("aeiou")
				Interpreted relative to the current
				case-sensitivity lexical context.

    (* <sre> ...)		0 or more matches
    (+ <sre> ...)		1 or more matches
    (? <sre> ...)  		0 or 1 matches
    (= <n> <sre> ...)		<n> matches
    (>= <n> <sre> ...)		<n> or more matches
    (** <n> <m> <sre> ...)	<n> to <m> matches
	<n> and <m> are Scheme expressions producing non-negative integers.
	<m> may also be #f, meaning "infinity"

    (|  <sre> ...)		Choice (OR is R5RS symbol; | is unspecified)
    (or <sre> ...)

    (:   <sre> ...)		Sequence (SEQ is Common Lisp symbol)
    (seq <sre> ...)		

    (submatch <sre> ...)		Numbered submatch
    (dsm <pre> <post> <sre> ...)	Deleted submatches
	<pre> and <post> are numerals.

    (uncase <sre> ...)			Case-folded match.

    (w/case   <sre> ...)		Introduce a lexical case-sensitivity
    (w/nocase <sre> ...)		context.

    ,@<exp>			Dynamically computed regexp.
    ,<exp>			Same as ,@<exp>, but no submatch info.
				    <EXP> must produce a character, string,
                                    char-set, or regexp.

    bos eos			Beginning/end of string
    bol eol			Beginning/end of line
    
    bow eow			Beginning/end of word
    (word  <sre> ...)		(: bow <sre> ... eow)
    (word+ <cset-sre> ...)	(word (+ (& (| alphanumeric "_")
                                            (| <cset-sre> ...))))
    word			(word+ any)

    (posix-string <string>)	Posix string -- for backwards compatibility.

    <char>			Singleton char set
    <class-name>		alphanumeric, whitespace, etc.
				    These two forms are interpreted subject to
				    the lexical case-sensitivity context.

    (~  <cset-sre> ...)		[^...] -- complement-of-union
    (-  <cset-sre> ...)		Difference
    (&  <cset-sre> ...)		Intersection

    (/ <range-spec> ...)	Character range -- interpreted subject to
				the lexical case-sensitivy context.

<class-name> ::= any			.
	         nonl	    		[^\n]

		 (Posix character classes:)
		 lower-case upper-case alphabetic numeric alphanumeric
		 punctuation graphic whitespace printing control hex-digit

		 blank ascii		(Gnu character classes)

		 (Shorter equivalent nicknames:)
		 lower upper alpha digit num alnum alphanum punct graph
		 space white print cntrl xdigit hex ascii

<range-spec> ::= <string> | <char>
		 The chars are taken in pairs to form inclusive ranges.


The ~, -, &, and word+ operators may only be applied to SRE's that specify
character sets. Here are the "type-checking" rules:

<cset-sre> ::= (~ <cset-sre> ...)		Set complement-of-union
	     | (- <cset-sre> ...)		Set difference
	     | (& <cset-sre> ...)		Intersection
	     | (| <cset-sre> ...)		Set union
	     | (/ <range-spec> ...)		Range

	     | (<string>)			Constant set
	     | <char>				Singleton constant set
	     | <string>				For 1-char string "c"

	     | <class-name>			Constant set

	     | ,<exp>				<exp> evals to a char-set,
	     | ,@<exp>				char, single-char string,
    						or re-char-set regexp.

	     | (uncase <cset-sre>)		Case-folding
	     | (w/case <cset-sre>)		
	     | (w/nocase <cset-sre>)		


-------------------------------------------------------------------------------
* Examples
----------

(- alpha ("aeiouAEIOU"))		; Various forms of non-vowel letter
(- alpha ("aeiou") ("AEIOU"))
(w/nocase (- alpha ("aeiou")))
(- (/"azAZ") ("aeiouAEIOU"))
(w/nocase (- (/"az") ("aeiou")))

;;; Upper-case letter, lower-case vowel, or digit
(| upper ("aeiou") digit)
(| (/"AZ09") ("aeiou"))

;;; Not an SRE, but Scheme code containing some embedded SREs.
(let* ((ws (rx (+ whitespace)))			; Seq of whitespace
       (date (rx (: (| "Jan" "Feb" "Mar" ...)	; A month/day date.
	            ,ws
                    (| ("123456789")	    	; 1-9
		       (: ("12") digit)    	; 10-29
		       "30" "31")))))		; 30-31

  ;; Now we can use DATE several times:
  (rx ... ,date ... (* ... ,date ...)	    
      ... .... ,date))

;;; More Scheme code
(define (csl re)		; A comma-separated list of RE's is
  (rx (| ""			; either zero of them (empty string), or
	 (: ,re 		; one RE, followed by
            (* ", " ,re)))))	; Zero or more comma-space-RE matches.

(csl (rx (| "John" "Paul" "George" "Ringo")))


-------------------------------------------------------------------------------
* A short tutorial
------------------
S-expression regexps are called "SRE"s. Keep in mind that they are *not*
Scheme expressions; they are another, separate notation that is expressed
using the underlying framework of s-expression list structure -- lists,
symbols, etc. SRE's can be *embedded* inside of Scheme expressions using
special forms that extend Scheme's syntax; there are places in the SRE 
grammar where one may place a Scheme expression -- in these ways, SRE's and
Scheme expressions can be intertwined. But this isn't fundamental; SRE's may
be used in a completely Scheme-independent context. By simply restricting
the notation to eliminate two special Scheme-embedding forms, they can be
a completely independent notation.


** Constant strings

The simplest SRE is a string, denoting a constant regexp. For example, the SRE
    "Spot"
matches only the string <<capital-S, little-p, little-o, little-t>>. There is
no interpretation of the characters in the string at all -- the SRE
    ".*["
matches the string <<period, asterisk, open-bracket>>.


** Simple character sets

To specify a set of characters, write a list whose single element is
a string containing the set's elements. So the SRE
    ("aeiou")
only matches a vowel. One way to think of this, notationally, is that the
set brackets are (" and ").


** Wild card

Another simple SRE is the symbol ANY, which matches any single character --
including newline and ASCII nul.


** Sequences

We can form sequences of SRE's with the SRE (: <sre> ...).
So the SRE
    (: "x" any "z")
matches any three-character string starting with "x" and ending with "z".
As we'll see shortly, many SRE's have bodies that are implicit sequences of
SRE's, analogous to the manner in which the body of a Scheme LAMBDA or LET
expression is an implicit BEGIN sequence. The regexp (seq <sre> ...) is
completely equivalent to (: <sre> ...); it's included in order to have a 
syntax that doesn't require : to be a legal symbol (e.g., for Common Lisp).


** Choices

The SRE (| <sre> ...) is a regexp that matches anything any of the
<sre> regexps match. So the regular expression
    (| "sasha" "Pete")
matches either the string "sasha" or the string "Pete". The regexp
    (| ("aeiou") ("0123456789"))
is the same as
    ("aeiou0123456789")
The regexp (or <sre> ...) is completely equivalent to (| <sre> ...); 
it's included in order to have a syntax that doesn't require | to be a 
legal symbol.


** Repetition

There are several SRE forms that match multiple occurences of a regular
expression. For example, the SRE (* <sre> ...) matches zero or more
occurences of the sequence (: <sre> ...). Here is the complete list
of SRE repetition forms:

SRE					Means 		At least    no more than
---					--------	--------    -----------
(* <sre> ...)				zero-or-more	0	    infinity
(+ <sre> ...)				one-or-more	1	    infinity
(? <sre> ...)				zero-or-one	0	    1
(= <from> <sre> ...)			exactly-n	<from>	    <from>
(>= <from> <sre> ...)			n-or-more	<from>	    infinity
(** <from> <to> <sre> ...)		n-to-m		<from>	    <to>

A <FROM> field is a Scheme expression that produces an integer.
A <TO> field is a Scheme expression that produces either an integer,
or false, meaning infinity.

While it is illegal for the <from> or <to> fields to be negative, it *is*
allowed for <from> to be greater than <to> in a ** form -- this simply
produces a regexp that will never match anything.

As an example, we can describe the names of car/cdr access functions
("car", "cdr", "cadr", "cdar", "caar" , "cddr", "caaadr", etc.) with
either of the SREs
    (: "c" (+ (| "a" "d")) "r")
    (: "c" (+ ("ad")) "r")
We can limit the a/d chains to 4 characters or less with the SRE
    (: "c" (** 1 4 ("ad")) "r")

Some boundary cases:
    (** 5 2 "foo")	; Will never match
    (** 0 0 "foo")	; Matches the empty string


** Character classes

There is a special set of SRE's that form "character classes" -- basically, 
a regexp that matches one character from some specified set of characters.
There are operators to take the intersection, union, complement, and
difference of character classes to produce a new character class. (Except 
for union, these capabilities are not provided for general regexps as they 
are computationally intractable in the general case.)

A single character is the simplest character class -- #\x is a character
class that matches only the character "x".  A string that has only one
letter is also a character class -- "x" is the same SRE as #\x.

The character-set notation (<string>) we've seen is a primitive character
class, as is the wildcard ANY. When arguments to the choice operator, |, are
all character classes, then the choice form is itself a character-class. 
So these SREs are all character-classes:
    ("aeiou")
    (| #\a #\e #\i #\o #\u)
    (| ("aeiou") ("1234567890"))
However, these SRE's are *not* character-classes:
    "aeiou"
    (| "foo" #\x)

The (~ <cset-sre> ...) char class matches one character not in the specified 
classes:
    (~ ("0248") ("1359"))
matches any character that is not a digit. 

More compactly, we can use the / operator to specify character sets by giving
the endpoints of contiguous ranges, where the endpoints are specified by a
sequence of strings and characters.  For example, any of these char classes
    (/ #\A #\Z  #\a #\z  #\0 #\9)
    (/ "AZ" #\a #\z "09")
    (/ "AZ" #\a "z09")
    (/"AZaz09")
matches a letter or a digit. The range endpoints are taken in pairs to
form inclusive ranges of characters. Note that the exact set of characters
included in a range is dependent on the underlying implementation's 
character type, so ranges may not be portable across different implementations.

There is a wide selection of predefined, named character classes that may be
used. One such SRE is the wildcard ANY. NONL is a character class matching
anything but newline; it is equivalent to
    (~ #\newline)
and is useful as a wildcard in line-oriented matching.

There are also predefined named char classes for the standard Posix and Gnu
character classes:
    scsh name		Posix/ctype	Alt name    Comment
    -------------------------------------------------------
    lower-case		lower
    upper-case		upper
    alphabetic		alpha
    numeric		digit		num
    alphanumeric	alnum		alphanum
    punctuation		punct
    graphic		graph
    blank					    (Gnu extension)
    whitespace		space		white	    ("space" is deprecated.)
    printing		print
    control		cntrl
    hex-digit		xdigit		hex
    ascii					    (Gnu extension)
See the scsh character-set documentation or the Posix isalpha(3) man page
for the exact definitions of these sets.

You can use either the long scsh name or the shorter Posix and alternate names
to refer to these char classes. The standard Posix name "space" is provided,
but deprecated, since it is ambiguous. It means "whitespace," the set of
whitespace characters, not the singleton set of the #\space character.
If you want a short name for the set of whitespace characters, use the
char-class name "white" instead.

Char classes may be intersected with the operator (& <cset-sre> ...), and
set-difference can be performed with (- <cset-sre> ...). These operators are
particularly useful when you want to specify a set by negation *with respect
to a limited universe*. For example, the set of all non-vowel letters is
    (- alpha ("aeiou") ("AEIOU"))
whereas writing a simple complement 
    (~ ("aeiouAEIOU"))
gives a char class that will match any non-vowel -- including punctuation,
digits, white space, control characters, and ASCII nul.

We can *compute* a char class by writing the SRE 
    ,<cset-exp>
where <cset-exp> is a Scheme expression producing a value that can be
coerced to a character set: a character set, character, one-character
string, or char-class regexp value. This regexp matches one character
from the set.

The char-class SRE ,@<cset-exp> is entirely equivalent to ,<cset-exp>
when <cset-exp> produces a character set (but see below for the more
general non-char-class context, where there *is* a distinction between
,<exp> and ,@<exp>).

Example: An SRE that matches a lower-case vowel, upper-case letter, 
or digit is
    (| ("aeiou") (/"AZ09"))
or, equivalently
    (| ("aeiou") upper-case numeric)

Boundary cases: the empty-complement char class
    (~)
matches any character; it is equivalent to 
    any
The empty-union char class
    (|)
never matches at all. This is rarely useful for human-written regexps,
but may be of occasional utility in machine-generated regexps, perhaps
produced by macros.

The rules for determining if an SRE is a simple, char-class SRE or a
more complex SRE form a little "type system" for SRE's. See the summary
section preceding this one for a complete listing of these rules.

** Case sensitivity

There are three forms that control case sensitivity:
    (uncase   <sre> ...)
    (w/case   <sre> ...)
    (w/nocase <sre> ...)

UNCASE is a regexp operator producing a regexp that matches any
case permutation of any string that matches (: <sre> ...).
For example, the regexp
    (uncase "foo")
matches the strings foo, foO, fOo, fOO, Foo, ...

Expressions in SRE notation are interpreted in a lexical case-sensitivy
context. The forms W/CASE and W/NOCASE are the scoping operators for this
context, which controls how constant strings and char-class forms are
interpreted in their bodies. So, for example, the regexp
    (w/nocase "abc"
              (* "FOO" (w/case "Bar"))
	      ("aeiou"))
defines a case-insensitive match for all of its elements except for the
sub-element "Bar", which must match exactly capital-B, little-a, little-r.
The default, the outermost, top-level context is case sensitive.

The lexical case-sensitivity context affects the interpretation of
    - constant strings, such as "foo"
    - chars, such as #\x
    - char sets, such as ("abc")
    - ranges, such as (/"az")
that appear within that context. It does not affect dynamically computed
regexps -- ones that are introduced by ,<exp> and ,@<exp> forms. It does
not affect named char-classes -- presumably, if you wrote LOWER, you didn't
mean ALPHA.

UNCASE is *not* the same as W/NOCASE. To point up one distinction,
consider the two regexps
    (uncase   (~ "a"))
    (w/nocase (~ "a"))
The regexp (~ "a") matches any character except "a" -- which means it *does*
match "A". Now, (uncase <re>) matches any case-permutation of a string that
<re> matches. (~ "a") matches "A", so (uncase (~ "a")) matches "A" and "a"
-- and, for that matter, every other character. So (uncase (~ "a")) is
equivalent to ANY.

In contrast, (w/nocase (~ "a")) establishes a case-insensitive lexical
context in which the "a" is interpreted, making the SRE equivalent to
(~ ("aA")).


** Dynamic regexps

SRE notation allows you to compute parts of a regular expressions
at run time. The SRE
    ,<exp>
is a regexp whose body <exp> is a Scheme expression producing a
string, character, char-set, or regexp as its value. Strings and
characters are converted into constant regexps; char-sets are converted
into char-class regexps; and regexp values are substituted in place.
So we can write regexps like this
    (: "feeding the "
       ,(if (> n 1) "geese" "goose"))
This is how you can drop computed strings, such as someone's name,
or the decimal numeral for a computed number, into a complex regexp.

If we have a large, complex regular expression that is used multiple
times in some other, containing regular expression, we can name it, using 
the binding forms of the embedding language (e.g., Scheme), and refer to
it by name in the containing expression. E.g.: The Scheme expression

    (let* ((ws (rx (+ whitespace)))			; Seq of whitespace

	   (date (rx (: (| "Jan" "Feb" "Mar" ...)	; A month/day date.
		        ,ws
                        (| ("123456789")	    	; 1-9
			   (: ("12") digit)	    	; 10-29
			   "30" "31")))))		; 30-31

      ;; Now we can use DATE several times:
      (rx ... ,date ... (* ... ,date ...)	    
          ... .... ,date))
        
where the (RX <sre> ...) macro is the Scheme special form that produces
a Scheme regexp value given a body in SRE notation.

As we saw in the char-class section, if a dynamic regexp is used
in a char-class context (e.g., as an argument to a ~ operation),
the expression must be coercable not merely to a general regexp,
but to a character set -- so it must be either a singleton string,
a character, a scsh char set, or a char-class regexp.

We can also define and use functions on regexps in the host language. 
For example,

    (define (csl re)		; A comma-separated list of RE's is either
      (rx (| ""			; zero of them (empty string), or
             (: ,re		; RE followed by
                (* ", " ,re))))); zero or more comma-space-RE matches.

    (rx ... ,date ...
        ,(csl (rx (| "John" "Paul" "George" "Ringo")))
	...
        ... ,(csl date) ...)

I leave the extension of CSL to allow for an optional "and" between the last
two matches as an exercise for the interested reader (e.g., to match
"John, Paul, George and Ringo").

Note, in passing, one of the nice features of SRE notation: SREs can
be commented.

When we embed a computed regexp inside another regular expression with
the ,<exp> form, we must specify how to account for the submatches that
may be in the computed part. For example, suppose we have the regexp
    (rx (submatch (* "foo"))
        (submatch (? "bar"))
        ,(f x)
        (submatch "baz"))
It's clear that the submatch for the (* "foo") part of the regexp is
submatch #1, and the (? "bar") part is submatch #2. But what number
submatch is the "baz" submatch? It's not clear. Suppose the Scheme
expression (f x) produces a regular expression that itself has 3
subforms. Are these counted (making the "baz" submatch #6), or not
counted (making the "bar" submatch #3)?

SRE notation provides for both possibilities. The SRE
    ,<exp>
does *not* contribute its submatches to its containing regexp; it
has zero submatches. So one can reliably assign submatch indices to
forms appearing after a ,<exp> form in a regexp.

On the other hand, the SRE
    ,@<exp>
"splices" its resulting regexp into place, *exposing* its submatches
to the containing regexp. This is useful if the computed regexp is defined
to produce a certain number of submatches -- if that is part of <exp>'s
"contract."


** String, line, and word units

The regexps BOS and EOS match the empty string at the beginning and end of
the string, respectively.

The regexps BOL and EOL match the empty string at the beginning and end of
a line, respectively. A line begins at the beginning of the string, and
just after every newline character. A line ends at the end of the string,
and just before every newline character. The char class NONL matches any
character except newline, and is useful in conjunction with line-based
pattern matching.

The regexps BOW and EOW match the empty string at the beginning and end of
a word, respectively. A word is a contiguous sequence of characters that
are either alphanumeric or the underscore character. 

The regexp (WORD <sre> ...) surrounds the sequence (: <sre> ...)
with bow/eow delimiters. It is equivalent to
    (: bow <sre> ... eow)

The regexp (WORD+ <cset-sre> ...) matches a word whose body is one or
more word characters also in one of the <class-arg> classes. It is equivalent 
to
    (word (+ (& (| alphanumeric "_")
                (| <cset-sre> ...))))
For example, a word not containing x, y, or z is
    (word+ (~ ("xyz")))

The regexp WORD matches one word; it is equivalent to 
    (word+ any)

[Note: BOL and EOL are not supported by scsh's current regexp search engine,
which is Spencer's Posix matcher. This is the only element of the notation
that is not supported by the current scsh reference implementation.]


** Miscellaneous elements

*** Posix string notation

The SRE (posix-string <string>), where <string> is a string literal
(*not* a general Scheme expression), allows one to use Posix string
notation for a regexp. It's intended as backwards compatibility and
is deprecated. Example:  (posix-string "[aeiou]+|x*|y{3,5}") matches 
a string of vowels, a possibly empty string of x's, or three to five
y's.


*** Deleted submatches

DSM's are a subtle feature that are never required in expressions written
by humans. They can be introduced by the simplifier when reducing
regular expressions to simpler equivalents, and are included in the
syntax to give it expressibility spanning the full regexp ADT. They
may appear when unparsing simplified regular expressions that have
been run through the simplifier; otherwise you are not likely to see them.
Feel free to skip this section.

The regexp simplifier can sometimes eliminate entire sub-expressions from a
regexp. For example, the regexp
    (: "foo" (** 0 0 "apple") "bar")
can be simplified to
    "foobar"
since (** 0 0 "apple") will always match the empty string. The regexp
    (| "foo"
       (: "Richard" (|) "Nixon")
       "bar")
can be simplified to
    (| "foo" "bar")
The empty choice (|) can't match anything, so the whole
    (: "Richard" (|) "Nixon")
sequence can't match, and we can remove it from the choice.

However, if deleting part of a regular expression removes a submatch
form, any following submatch forms will have their numbering changed,
which would be an error. For example, if we simplify
    (: (** 0 0 (submatch "apple"))
       (submatch "bar"))
to
    (submatch "bar")
then the "bar" submatch changes from submatch 2 to submatch 1 -- so this
is not a legal simplification.

When the simplifier deletes a sub-regexp that contains submatches,
it introduces a special regexp form to account for the missing,
deleted submatches, thus keeping the submatch accounting correct.
    (dsm <pre> <post> <sre> ...)
is a regexp that matches the sequence (: <sre> ...). <Pre> and
<post> are integer constants. The DSM form introduces <pre> deleted
submatches before the body, and <post> deleted submatches after the
body. If the body (: <sre> ...) itself has body-sm submatches,
then the total number of submatches for the DSM form is
    (+ <pre> body-sm <post>)
These extra, deleted submatches are never assigned string indices in any
match values produced when matching the regexp against a string.

As examples,
    (| (: (submatch "Richard") (|) "Nixon")
       (submatch "bar"))
can be simplified to
    (dsm 1 0 (submatch "bar"))

The regexp
    (: (** 0 0 (submatch "apple"))
       (submatch "bar"))
can be simplified to
    (dsm 1 0 (submatch "bar"))


** Embedding regexps within Scheme programs

SRE's can be placed in a Scheme program using the (rx <sre> ...) 
Scheme form, which evaluates to a Scheme regexp value.


** Static and dynamic regexps

We separate SRE expressions into two classes: static and dynamic
expressions. A *static* expression is one that has no run-time dependencies;
it is a complete, self-contained description of a regular set. A *dynamic*
expression is one that requires run-time computation to determine the
particular regular set being described. There are two places where one can
embed run-time computations in an SRE:
    - The <from> or <to> repetition counts of **, =, and >= forms;
    - ,<exp> and ,@<exp> forms.
A static SRE is one that does not contain any ,<exp> or ,@<exp> forms, 
and whose **, =, and >= forms all contain constant repetition counts.

Scsh's RX macro is able, at macro-expansion time, to completely parse,
simplify and translate any static SRE into the equivalent Posix string
which is used to drive the underlying C-based matching engine; there is 
no run-time overhead. Dynamic SRE's are partially simplified and then expanded
into Scheme code that constructs the regexp at run-time.


-------------------------------------------------------------------------------
* Discussion and design notes
-----------------------------
Many considerations drove the design of the SRE notation. I took
advantage of ideas found in the s-expression notations of Manuel 
Serrano's Bigloo systems-programming Scheme implementation and Michael
Sperber's Scheme 48 design. I also considered features found in many
traditional regexp implementations, including the Posix standard, Henry 
Spencer's package, Tom Lord's rx system, gnu regex, and Perl's system. 
Features that didn't make it into the scsh system are not there for a reason.

Lord's package provided the name for the basic SRE macro, which is
agreeably brief.

Another late-breaking related system: the day prior to the release of this
document, I was pointed to a similar new package that has been implemented for
gnu emacs, Bob Glickstein's sregex.el.  Sregex.el's notation is strikingly
similar to SRE, but SRE notation seems to be a superset in functionality, and
it provides a real ADT for run-time computation of regexps instead of using
the list-structure form of the expression (this is discussed in more detail
below).

What follows is a loose collection of design notes.

** No lazy repetition operators

SRE notation does not provide lazy repeat forms, of the kind found in perl.
Lazy repeat forms have problems. In principle, a regexp doesn't specify a
matching algorithm, it specifies a *set of strings*. Lazy submatches violate
this principle. Providing lazy submatches forces a particular matching
algorithm: an NDFA-based backtracking search engine -- you can't use a
fast DFA no-backtracking engine. I chose to restrict the notation to keep
it algorithm-independent. (This isn't strictly true -- we sleaze in one area:
submatches require an NDFA searcher, but the standard C-based DFA engines
have clever hacks for dealing with this.)

Also, lazy submatches can't be implemented with a Posix search engine,
and I wanted to allow for this.

Note that an alternative to perl's lazy repeat forms would be to have
a flag in the match and search functions telling it to provide either the
leftmost longest or the leftmost shortest match. This is a *global* property
of the whole regexp, not of a particular part, and can be easily provided
by either DFA or NDFA engines.

I suspect this would handle many (most? all?) of the cases where perl hackers
want lazy repetition operators. But it is more in the spirit of regexps,
where one notates a *regular set* of strings with a regexp, then asks the
matcher to find a match -- it's the matcher's business how it will choose
from multiple matches to strings in the set.

Finally, my suspicion is that the sort of things people do with lazy
repetition operators (e.g., match delimited <foo> ... </foo> regions
in html text) are abusing regexps, pushing them beyond their real
capabilities, which is asking for trouble. Strings of balanced delimiters
aren't regular; you should be using more powerful parsing tools if this
is what you want to do. Don't break regexps to handle this case in a
fragile way; design a different parsing tool. For an example of a more
powerful parsing tool, see the elegant parser tool, READ/RP, in Serrano's
Scheme system bigloo.


** No named submatches

One might want a feature wherein we could *name* our submatches, instead
of referring to them by their numerical index in the regexp. Perhaps
something like
    (: ...
       (named-match phone-num (= 3 digit) "-" (= 4 digit))
       ...)
which would somehow "bind" phone-num to the substring matching the
seven-digit phone number. This is awkward. The problem is that binding
introduces issues of scope -- what is the scope of an identifier "bound"
in a regexp? Suppose the SRE is used in a Scheme form to produce a first-class
regexp value, which can be passed around in and out of various scopes?
Clearly, somehow using Scheme's variables isn't going to work. If one then
turns to symbol-indexed tables, one is leaving the language-level for
binding, and moving to run-time data values. This is inefficient and
awkward. Furthermore, what are the precedence rules when the same identifier
is bound multiple times in the same SRE?

There's no shame in positional, indexed references. It's how parameters
are passed in Scheme procedure calls. It's the natural mechanism that
arises for regexps, so that's what is provided.

One might consider a hairy named-submatch system where one would specify in
a *single form* (1) an SRE with named submatches, (2) a string to match/search,
and (3) a body of code to be conditionally evaluated in the scope of the bound
names. This would use names in a way that was tightly integrated with Scheme,
which is good. But you must now wrestle with some very tricky issues:

- Are the names bound to substrings or pairs of start/stop indices into
  the source text? Sometimes you want one, sometimes the other.

- You have to give up on passing regexps around as first-class values.
  SRE's are now scope-introducing, variable-binding syntax, like
  LET or LAMBDA. So much for ,<exp> and the power of dynamic regexps.

- What to do about a name that appears multiple times in the regexp?

I did not go down this path. Be one with the system; don't fight against it.
(However, see the LET-MATCH, IF-MATCH, and MATCH-COND forms, which provide
matching, positional name-binding, and control transfers in a consistent
manner.)


** No intersection or negation operators

The SRE notation supports a general union operator, the choice form
(| <sre> ...). However, the rest of the set algebra -- negation, 
intersection, and subtraction -- is restricted to character sets. Wouldn't 
it be nice to extend these operators to apply to general regexps? After
all, regular sets *are* closed under these operations, and they are useful, 
so they should be in the toolkit.

They aren't in for two reasons:
1. Combinatoric explosion
   Intersection forces you to convert your regexp to a DFA, which can entail
   exponential growth in the state space. After the exponential explosion,
   you work in the cross-product of the two DFA's state spaces, for another
   multiplicative factor. It's hard to control. Negation presents similar
   difficulties, in either time (DFA) or space (NDFA).

2. Standard C engines don't provide it. (Because of reason #1.)

Sure, it'd be great to have them anyway. What you probably want is
a direct-in-Scheme DFA/NDFA toolkit. Then you can take your regexp,
convert it to a DFA, do the intersection, and either interpret the
result machine, or translate it to Scheme code for direct execution.
The programmer would have to take responsibility for managing the
potential for combinatorial explosion.

It's a great idea. You do it. I was careful to design the notation to
allow for it -- you don't even have to introduce new operators, just
lift the char-class "typing" restriction on the existing ~, -, and &
ops.

Note that just by allowing general set operations on character classes, 
we're still way out in front of traditional notation, which doesn't 
provide this feature at all.


** No name-binding forms

SRE notation doesn't have a form for binding names to regexps; this
is punted to the host language by way of the ,<exp> mechanism. This
is arguably a lack from the point of view of SRE's as a completely
standalone, static notation, independent of their embedding language. 
But enough is enough.


** No SRE macros

It's a shame that we can't provide a means of allowing programmers to define
their own SRE macros, by which I do *not* mean Scheme expressions that contain
SRE forms, but *new* classes of SRE form, beyond (: ...), (| ...) and friends.
For example, the (WORD+ <cset-sre> ...) form is not really primitive; it
can be defined by way of expansion to
    (: bow (& (| alphanum "_") (| <cset-sre> ...)) eow)
A given task might profit from allowing the programmer to extend SRE's
by way of rewriting forms into the base SRE notation. But Scheme does
not support this -- it only provides macros for Scheme expressions.
Too bad.


** No back-references

Completely non-regular -- there's a reason these were dropped from Posix'
"extended" (= "modern") regexp notation.


** No "syntax classes"

"Syntax classes" are a gnu-emacs feature for describing certain character
sets. SRE already has a powerful set of character-set operators, and the
whole notion of "syntax class" is emacs specific. So they weren't included.


** Range notation

One might consider it more "Schemey" to have the char-class range notation
specify the from/to pairs as two-element lists, e.g.
    (/ (#\a #\z) (#\0 #\9))
or maybe even
    (/ (#\a . #\z) (#\0 . #\9))
If we do things this way, the structure of the s-expression more closely
mirrors the underlying structure of the form. Well, yes. But it's hard
to read -- I claim that ripping out all the sharp signs, backslashes, dots
and extra parens is much easier on your eyes:
    (/"az09")
and I am unable to see what the extra pairing overhead buys you over
and above gratuitous notational bloat. Remember that this notation is
designed primarily for *human* producers and consumers. The *machine*
doesn't see the *notation*; it sees the nice, regular ADT.


** Big notation

SRE notation is baroque -- there are a lot of ways to write the same
regexp. This is not accidental. The idea is to make a notation that is as
expressive as possible for human-written documents. The ADT, in contrast, is
simple and spare -- it is designed to be operated upon by programs.


** Implementation complexity

This implementation is much more complex than I'd like; there are three main
reasons. The first reason is the strategy of parsing SRE's, not directly to
Scheme code, but instead to regexp records, which can then be simplified and
then unparsed to Scheme code (or other forms, such as SRE or Posix string).
Centering the design on the ADT was the right thing to do, as it enables other
unparsers to use the same "front-end" parser -- for example, if someone wanted
to write a macro that expanded (static) SRE's into a LETREC directly
implementing the DFA, it would be much easier to work from the ADT than
directly from the SRE form. It also enables the macro to apply a lot of
processing to the form at compile-time, such as the simplifier, giving us a
sleazy but effective form of partial evaluation.  However... we pay a price in
complexity to do things this way. Code that processes regexp records that
might be used *at macro-expansion time* must be written to tolerate the
presence of Scheme *code* in record fields that would ordinarily only contain
Scheme *values* -- for example, the FROM and TO fields of the repeat record. I
got a lot of code reuse by making these records do double-duty as both the
regexp ADT *and* the compiler's syntax tree (AST) for expressions computing
these values -- but I had to get the code that did double-duty *just right*,
which meant being careful to add code/value checks on all accesses to these
fields.

(Note that *user* code manipulates run-time regexp values, and so will
never see anything but values in these fields.)

The second reason is the inherent difficulty in translating general character
sets to [...] Posix character classes, which seem simple at first, but turn
out to have very awkward special-case restrictions on the grammar -- I discuss
this at length in a later section. The char-set rendering code is made more
complex than it could have been because I made an effort to render them
into concise, readable descriptions, using ranges and ^ negation where
possible to minimise the notation.

The final major influence on the code complexity is all the bookkeeping that
is involved in submatches and DSM's. Tracking DSM's complicates just about all
parts of the system: the data-structures, the simplifier, the parser, and so
on. It's really amazing how this one feature comes to dominate all the
processing. But it has to be done. Submatches are an indispensable part of the
way we use regexps. Simplification -- the process that introduces DSM's -- is
not an option: SRE syntax is more general than traditional syntax, and permits
authors to write expressions that don't have representations in traditional
syntax. Simplifying the regular expression rewrites these un-translatable
cases into equivalent cases that *are* translatable. So we must simplify
before translating to Posix strings, hence we are stuck with DSM's.

I went to the trouble of doing a full-bore implementation to have a
reference implementation for others to steal. So the complexity of the
coding shouldn't throw anyone who wants to use this notation; it's all
been implemented.


** Should anchors be primitive regexps or operators?

There are two ways to do anchors, such as BOL and EOL, in an
s-expression syntax. One way is to have them be primitive regexps
that match the empty string in a particular context (which is how it
is done in SRE syntax). The alternate method, found in some other designs
I have studied, is to have anchors be *operators* with bodies, e.g.
    (at-bos "foo" (* " ") "bar")
which would match a "foo<white-space>bar" string, anchoring the match
to the beginning of the string. This works reasonably well for 
beginning-of-element anchors, but with end-of-element anchors, it puts
the operator on the wrong side -- the left side -- of the regexp body:
    (at-eol "foo" (* " ") "bar")
The end-of-line in this pattern occurs after the "bar", but the operator's
way over on the other side of the regexp body. This gets especially ugly 
when we want to delimit both sides of the body:
    (at-eol (at-bos "foo" (* " ") "bar"))
Too many parens; too much nesting; too hard to read. I went with the
magic-empty-string primitive regexp model:
    (: bos "foo" (* " ") "bar" eos)


** Character class operators

In an earlier version of this notation, I had a distinct subnotation
for character classes, with a distinct non-| operator for char-set
union. This provided a simple, syntactic way to separate the char-set
set algebra from the other operations of the language, to ensure you
didn't try to complement or intersect general regexps. The char-set
operators were IN, -, &, ~ for union, difference, intersection, and 
complement. Inside these operators, simple strings stood for character
sets. So we'd write the any-vowel SRE (in "aeiou") instead of ("aeiou").

Shifting over to a distinct SRE form for constant char sets -- ("aeiou") --
allowed me throw out the whole syntactic division between char-sets and
other regexps, replacing this division with a "type system" restricting
non-union set operations to char-sets. This seems like a big improvement
for two reasons:
    - The syntax was simplified and made less context-dependent.
    - Should we ever wish to extend the regexp system to allow
      for set operations on general regexps, the notation doesn't
      have to be changed or extended at all.


** Operator names

Always a painful task. Here are some random notes on my choices.

Before choosing W/CASE and W/NOCASE, I considered
    CASE-SENSITIVE and CASE-INSENSITIVE 
and
    CASE0 CASE1
but the former is far too long, and the latter was insufficiently clear.
One early reviewer (Rees) asked me where were CASE2, CASE3, et al. So I went
with W/CASE and W/NOCASE.

I considered CASE-FOLD before selecting UNCASE. UNCASE is shorter and
seems no more or less clear.

I considered &/+/~/- for the set algebra ops -- it's a nice, consistent, terse
operator set. This would give us an R5RS-sanctioned operator for union, as an
alternative to the slightly iffy |.  But if we use + for union, there is some
pressure to use * for intersection, by way of analogy with mod-2
addition/multiplication. But * is already assigned to a repetition operator.
Furthermore, + is also already taken, by the 1..infinity repetition
operator. Not only are these * and + assignments firmly ingrained in
traditional syntax, I couldn't come up with a good, short alternative for the
repetition operators, so I kept them + and *. | is also the and/or mate of &
in C, so it's natural to pair them in an and/or intersection/union manner.

I also considered and/or/not/diff for the char-set algebra ops. But
the names are too long for such common primitives.


** No collating elements

Posix has this completely opaque feature of character sets called "collating
elements." It's some mechanism whereby you can, in a locale-independent way,
get a pair of characters to sort as one character, or have a German strasse
(the one that looks like a beta) character sort like a pair of s's. This stuff,
which I am unable to understand without bleeding from the nose and ears, is
part of the full Posix regexp spec -- you can say things like [[=ch=]]
and get a character class which will actually match two characters out of
a target string, if your locale defines a collating element <ch>.

My system doesn't support this at all. SRE char-class expressions are
rendered to scsh character sets, which are then rendered into [...]
sets containing the elements of those sets. The rendered [...] expressions
never contain [..], [==], or [::] elements.

As I discuss below, I'm willing to support a super-ASCII character type,
such as Unicode or big-5 or latin-1. What I find objectionable is the
idea of "collation elements" that can match more than one character in
a row.

I'm open to more suggestions on this front.


** Character set dependencies

If you wanted to do a Unicode version of my package, you'd have to redo the
scsh character-set machinery, and also the char-set unparsers in the regexp
backends.  If you wanted to do a Latin-1 version, you'd need to slightly tune
scsh's primitive character sets (such as char-set:alphabetic).  The SRE->Posix
string character-class rendering code is written to be as independent as
possible of the character type, but it has some dependencies, and would need
to be tuned.

This is straightforward to do and makes sense in the global context in which
we now program. However, I'm an American (= functionally monolingual), and so
not as expert on the various internationalisation issues as a European or
Asian would be -- so I'm punting it for now.


** Problems with traditional regexps

SRE notation was intended to fix a lot of things I didn't like about
working with traditional notation.


*** Traditional regexp notation doesn't scale over large regexps

Traditional regexp notation has a lot of problems, and the bigger
the regexp, the worse they get:
- They can't be laid out to express their structure with indentation.
- They can't be commented.
- They don't have an abstraction mechanism -- parts can't be named
  and used, functions can't be defined. (This can be hacked using
  mechanisms like sprintf(), but it is awkward and error prone.)


*** String constants

There's no need to backquote special characters in SRE string constants.
Just write them down in Scheme string-literal syntax.


*** Traditional regexp notation doesn't scale to rich operator sets

Traditional notation tries to use single characters as its operators:
. * ( ) ^ $ and so forth. Unfortunately, the more chars you reserve as
operators, the more backslash-quoting you have to do when you write down 
constant strings. Eventually, you run out of special characters. Using a 
single special character to prefix operators (as Gnu regexps do with 
backslash) rapidly renders regexps unreadable -- especially when these
backslashes have to be doubled to get them into the host language's string
literals. When regexp packages such as Gnu, Perl, or Spencer start to expand 
their operator repertoires, they are forced to adopt very unwieldy syntactic
mechanisms. For example, Spencer's notation for beginning-of-word and 
end-of-word boundaries are [[:<:]] and [[:>:]], a somewhat bizarre bit of
syntactic jiggering.

S-expressions, on the other hand, are a little more verbose for
simple forms, but paying this cost up-front gets you into a general
framework that is extremely extensible. It's easy to add many new
operators to the SRE syntax -- as a result, SRE can be a very rich syntax.
You choose:
    SRE:     (w/nocase (word+ (~ ("aeiou"))))
    Brand X: "[[:<:]]([b-df-hj-np-tv-zB-DF-HJ-NP-TV-Z])+[[:>]]"
Note that not one of the three operators used in the SRE version is
available in traditional notation. That tells the story right there.

*** Traditional regexp [...] classes 

There is a slew of special cases in the Posix grammar for [...] classes
to shoehorn [...]'s special chars (carat, right bracket, and hyphen) into the
notation as set elements. Examples:
- Right bracket terminates a char class... unless it's the first
  character following the left bracket... or the second character, 
  following an initial ^. 
- To put in a carat, place it at the *end* (well, perhaps *next* to the
  end, if you also want to put in a hyphen) -- unless it's the only element,
  then just punt the whole [...], and write it as a character. 
- To put in a minus sign, really place it at the end, even after a carat.
  Unless, that is, the whole char set is just the two characters carat
  and hyphen, in which case you'd have [^-], which would mean something
  else entirely -- so in this one case, flip the order of the two characters,
  and put hyphen first: [-^].
- Be sure never to accidentally place a left-bracket element next
  to an equals sign, colon, or period, because [=, [:, and [. are
  collating element open-delimiters in Posix regexps -- you can
  say things like "[ABC[:lower:]123]" to get A, B, C, 1, 2, 3 and
  all the lower-case letters. Better shuffle the class elements around
  to avoid these juxtapositions.
- There's no way, at all, to write the empty character class.
  ("[]" is not syntactically legal; if you try "[^\000-\177]", you will
  probably blow up your C regexp engine with the non-terminating nul
  byte, and, in any event, you are being ASCII specific.)

This is not even the whole set of special exceptions. You start putting
special characters into your [...] char classes, and you walk into a
mine field. Who designed this mess?

As evidence of this complexity, the code that translates general Scheme
character sets into this notation is the single largest and most complicated
part of the ADT->Posix-string compiler. Better, however, that the unparser code
puzzle out how to represent sets given all these ill-structured, error-prone
rules than for you to have to waste time thinking about it yourself, as
you must when you use traditional notation.

Even with its baroque syntactic rule set, the [...] construct is pretty
limited -- it lacks the compositional elements and general set operators
that SRE char classes have. Writing a [...] form that will match any
non-vowel letter is pretty painful because there is no set-difference
operator:
    [b-df-hj-np-tv-zB-DF-HJ-NP-TV-Z]
You'd have to stare at this for a minute to figure out what it is. The
corresponding SRE is much more transparent:
    (- alpha ("aeiouAEIOU"))


*** ASCII nul and newline

Does . match a ASCII nul? Does it match a newline? Does [^x]? Unforunately, 
the same expressions have different meanings depending on the implementation
and the flags passed to the pattern-compiler functions. In SRE notation,
the behaviour of each element is unambiguously defined. No surprises; no
misunderstandings.

*** Newline

Various regexp systems can never seem to agree on the treatment of newline. 
Is it matched by . or [^x]? Do the anchors ^ and $ match beginning/end of
line, or just beginning/end of string? Gnu regexps do it one way; Posix,
another. Posix provides a compile-time flag that shifts the meaning of 
all these constructs from string-oriented to line-oriented -- but multiplexing
the notation in this global way means you can't do a bit of each in the
same regexp.

SRE's don't have this problem, partly because they aren't restricted to
just using punctuation characters such as . [ ] ^ $ for operators. It was
straightforward to add both bos/eos/any *and* bol/eol/nonl. Say what you
mean; mean what you say.

(Unfortunately, I don't have an underlying C engine that *implements*
both eos/bos and bol/eol matching. But I've got a driver, and that's a
start.)


*** Submatch and grouping

Parens are overloaded in Posix syntax as both grouping operators and
as submatch markers. Some implementations extend the traditional
syntax with awkward non-submatch-introducing grouping parentheses,
but they are non-standard extensions and syntactically awkward.

S-expressions, on the other hand, have no precedence issues, so
grouping is distinct from the submatch operator. Removing these
spurious submatches can have huge performance benefits, since submatch
assignment rules out non-backtracking DFA search (roughly speaking).


*** Grammar ambiguity

The traditional syntax has a lot of squirrelly cases in the grammar
to trip up the unwary. Some examples:
- Is "x+*" the regexp (* (+ "x")) or is it (: (+ "x") (* ""))?
- Is "" an empty choice (SRE (|), which never matches anything), 
  or an empty sequence (SRE (:), which always matches the empty string)?

SRE syntax doesn't have any of these problems.


*** Grammar limitations

As we've seen, there's no way to write an empty match in Posix notation,
i.e. something that will never match.  [] is not an empty class, due to
bizarre special-casing of right-bracket in the special context of immediately
following a left bracket or left-bracket/carat.

This may not seem important, but the generality can be handy when the
regexp expressions are being computed, not hand-written (e.g., by a macro,
or when unparsing an ADT value).

Notations should provide coverage all the way out to the boundary cases.
When they fail to do this, someone, sooner or later, runs into trouble.

*** No exported ADT

It's hard to compute regexps with the string representation -- for example,
if you want to drop a constant string into a regexp, you have to write
a routine to quote the special chars. Code that wants to manipulate regexps
in terms of their structure is much harder to write in terms of the string
form than the structured ADT values. As one of Perlis' aphorisms states,

    The string is a stark data structure and everywhere it is passed 
    there is much duplication of process. It is a perfect vehicle for 
    hiding information.

Working in terms of the grammar keeps you one step removed from the level at
which you want to be operating, and the grammar doesn't even permit you write
down straighforward boundary cases, such as the empty choice, the empty
sequence, or the empty char class.


*** Comments

You can comment SRE notation. The traditional notation doesn't permit this.
This is a big problem for large, complex regexps. (To be fair, Perl's
non-standard syntax *does* permit comments to be interleaved with pieces 
of a regexp.)


*** Case sensitivity

Traditional notation has no support for case-sensitivity. While the Posix
pattern-compiler allows for a case-insensitive flag to be globally applied
to a whole pattern, this is distinct from the notation itself -- not the
right thing -- and does not provide for locality of scope.


-------------------------------------------------------------------------------
* Regexp functions
------------------

(string-match posix-re-string string [start]) -> match or false
(make-regexp posix-re-string) -> re
    Old functions for backwards compatibility. Will go away at some point in
    the future.

(rx sre ...)		Regexp macro
    This allows you to describe a regexp value with SRE notation.

(regexp? x) -> bool

(regexp-search  re str [start flags]) -> false or match-data
(regexp-search? re str [start flags]) -> boolean
    FLAGS is bitwise-or of regexp/bos-not-bol and regexp/eos-not-eol.
    regexp/bos-not-bol means the beginning of the string isn't a line-begin.
    regexp/eos-not-eol is analogous. [They're currently ignored because
    BOL and EOL aren't supported.]

    Use REGEXP-SEARCH? when you don't need submatch information, as
    it has the potential to be *significantly* faster on submatch-containing
    regexps.

    There is no longer a separate regexp "compilation" function; regexp
    records are compiled for the C engine on demand, and the resulting
    C structures are cached in the regexp structure after the first use.

(match:start m [i]) -> int or false
(match:end   m [i]) -> int or false
(match:substring m [i]) -> string or false

(regexp-substitute port-or-false match-data . items)
    An item is a string (copied verbatim), integer (match index),
    'pre (chars before the match), or 'post (chars after the match).
    #f for port means return a string.

    See the scsh manual for more details.

(regexp-substitute/global port-or-false re str . items)
    Same as above, except 'post item means recurse on post-match substring.
    If RE doesn't match STR, returns STR.

(let-match match-exp mvars body ...)			Syntax
(if-match  match-exp mvars on-match no-match)		Syntax
    MVARS is a list of vars that is bound to the match and submatches
    of the string; #F is allowed as a don't-care element. For example,
	(let-match (regexp-search date s)
                   (whole-date month day year)
          ...body...)
    matches the regexp against string s, then evaluates the body of the
    let-match in a scope where M is bound to the matched string, and
    SM2 is bound to the string matched by the second submatch.

    IF-MATCH is similar, but if the match expression is false, 
    then the no-match expression is evaluated; this would be an
    error in LET-MATCH.

(match-cond (<match-exp> <match-vars> <body> ...)	; As in if-match
            (test <exp> <body> ...)			; As in cond
            (test <exp> => <proc>)			; As in cond
            (else <body> ...))				; As in cond


(flush-submatches re) -> re	; Returned value has no submatches
(uncase re)           -> re	; Case-fold regexp
(simplify-regexp  re) -> re	; Simplify the regexp
(uncase-char-set cset) -> re
(uncase-string str) -> re


(sre->regexp sre) -> re		; S-expression parser
(regexp->sre re)  -> sre	; S-expression unparser

(posix-string->regexp string) -> re	; Posix regexp parser
(regexp->posix-string re) -> string	; Posix regexp unparser
    - The string parser doesn't handle the exotica of character class
      names such as [[:alnum:]]; I wrote in in three hours.
    - The unparser produces Spencer-specific strings for bow/eow
      elements; otherwise, it's Posix all the way.

    You can use these tools to map between scsh regexps and Posix
    regexp strings, which can be useful if you want to do conversion
    between SRE's and Posix form. For example, you can write a particularly
    complex regexp in SRE form, or compute it using the ADT constructors,
    then convert to Posix form, print it out, cut and paste it into a
    C or emacs lisp program. Or you can import an old regexp from some other
    program, parse it into an ADT value, render it to an SRE, print it out, 
    then cut and paste it into a scsh program.

-------------------------------------------------------------------------------
* The scsh regexp ADT 
---------------------
The following functions may be used to construct and examine scsh's
regexp abstract data type.

** Sequences
(re-seq? x) -> boolean				; Type predicate
(make-re-seq . re-list) -> re			; Basic constructor
(re-seq      . re-list) -> re			; Smart constructor
(re-seq:elts re) -> re-list			; Accessors
(re-seq:tsm  re) -> integer			; .

** Choices
(re-choice? x) -> boolean			; Type predicate
(make-re-choice re-list) -> re			; Basic constructor
(re-choice      . re-list) -> re		; Smart constructor
(re-choice:elts . re) -> re-list		; Accessors
(re-choice:tsm re) -> integer			; .

** Repetition
(re-repeat? x) -> boolean			; Type predicate
(make-re-repeat  from to body)			; Basic constructor
(re-repeat       from to body)			; Smart constructor
(re-repeat:body re) -> re			; Accessors
(re-repeat:from re) -> integer			; .
(re-repeat:to   re) -> integer			; .
(re-repeat:tsm  re) -> integer			; .

** Submatches
(re-submatch? x) -> boolean			; Type predicate
(make-re-submatch body [pre-dsm post-dsm])	; Basic constructor
(re-submatch      body [pre-dsm post-dsm])	; Smart constructor
(re-submatch:body     re) -> re			; Accessors
(re-submatch:pre-dsm  re) -> integer		; .
(re-submatch:post-dsm re) -> integer		; .
(re-submatch:tsm      re) -> integer		; .

** String constants
(re-string? x) -> boolean			; Type predicate
(make-re-string chars) -> re			; Basic constructor
(re-string chars) -> re				; Basic constructor
(re-string:chars re) -> string			; Accessor

** Char sets
(re-char-set? x) -> boolean			; Type predicate
(make-re-char-set cset) -> re			; Basic constructor
(re-char-set cset) -> re			; Basic constructor
(re-char-set:cset re) -> char-set		; Accessor

** DSM
(re-dsm? x) -> boolean				; Type predicate
(make-re-dsm  body pre-dsm post-dsm) -> re	; Basic constructor
(re-dsm       body pre-dsm post-dsm) -> re	; Smart constructor
(re-dsm:body re) -> re				; Accessor
(re-dsm:pre-dsm  re) -> integer			; .
(re-dsm:post-dsm re) -> integer			; .
(re-dsm:tsm      re) -> integer			; .

** Primitive regexps
re-bos re-eos re-bol re-eol re-bow re-eow	; Primitive regexps
re-bos? re-eos?					; Type predicates
re-bol? re-eol?
re-bow? re-eow?

trivial-re trivial-re?				; ""
empty-re   empty-re?				; (|)
re-any	   re-any?				; any
re-nonl						; (~ #\newline)
re-word						; word
    These are non-primitive predefined regexps of general utility.

(regexp? x) -> boolean
(re-tsm re) -> integer

(clean-up-cres)

-------------------------------------------------------------------------------
* Syntax-hacking tools
----------------------
The Scheme 48 package rx-syntax-tools exports several tools for macro
hackers that want to use SREs in their macros. In the functions defined
below, COMPARE and RENAME parameters are as passed to Clinger-Rees
explicit-renaming low-level macros.

(if-sre-form form conseq-form alt-form)					Syntax
    If FORM is a legal SRE, this is equivalent to the expression
    CONSEQ-FORM, otherwise it expands to ALT-FORM.
    
    This is useful for high-level macro authors who want to write a macro
    where one field in the macro can be an SRE or possibly something
    else. E.g., we might have a conditional form wherein if the
    test part of one arm is an SRE, it expands to a regexp match
    on some implied value, otherwise the form is evaluated as a boolean 
    Scheme expression:

       (if-sre-form test-exp			; If TEST-EXP is a regexp,
         (regexp-search? (rx test-exp) line)	; match it against the line,
         test-exp)				; otw it's a boolean exp.

(sre-form? form compare) -> boolean
    This procedure is for low-level macros doing things equivalent to
    IF-SRE-FORM. It returns true if the form is a legal SRE.

Note that neither of these tests does a deep recursion over the form
in the case where the form is a list. They simply check the car of the
form for one of the legal SRE keywords.

(parse-sre  sre-form  compare rename) -> re
(parse-sres sre-forms compare rename) -> re
    Parse SRE-FORM into an ADT. Note that if the SRE is dynamic -- 
    contains ,<exp> or ,@<exp> forms, or has repeat operators whose
    from/to counts are not constants -- then the returned ADT will have
    *Scheme expressions* in the corresponding slots of the regexp records
    instead of the corresponding integer, char-set, or regexp. In other
    words, we use the ADT as its own AST. It's called a "hack."

    PARSE-SRES parses a list of SRE forms that comprise an implicit sequence.

(regexp->scheme re rename) -> Scheme-expression
    Returns a Scheme expression that will construct the regexp RE
    using ADT constructors such as make-re-sequence, make-re-repeat,
    and so forth.

    If the regexp is static, it will be simplified and pre-translated
    to a Posix string as well, which will be part of the constructed
    regexp value.

(static-regexp? re) -> boolean
    Is the regexp a static one?

-------------------------------------------------------------------------------
* Acknowledgements
------------------
If you want to know precise details on obscure features of Posix regexps,
and their associated algorithms, you have to ask Tom Lord or Henry Spencer.
I did.

Alan Bawden was the one who proposed making | a "polymorphic" char-class/regexp
union operator, with an inference pass to disambiguate, thus leaving the SRE
syntax open to full intersection/union/difference/complement extension. I
found this to be an amazingly clever idea. Alan also explained some esoteric
points concerning low-level macros to me.
