Scanner Definitions
===================

Token References
----------------

Parse rules can reference terms in following ways:

1) By literal text (see `ParseAttribute.LiteralMask`):

`ref "foo" =requires=> exists t such (t by "foo")`

Token can be referenced by literal in following situations:

- there are no scan rules. token is defined implicitly
- there is one `[Literal]` scan rule for that literal

2) By token type in parse rule argument type:

    ref T    =require=> exists t such (t by T)
    Token can be identified by the type when there is one ore more 
    scan rules returning this type.

Legal scan rules
----------------

1. def t "foo" T       => t by "foo", T

    [Lieral("foo")]
    T Foo();

2. def t "foo" object  => t by "foo"

    [Lieral("foo")
    object Foo();

3. def t /patt/ T      => t by T, t by-nolit

    [Scan("patt")
    T PattMatch();

Scan Rule Conflicts
-------------------

Single scan rule definition does not produce conflicts.
Conflicts are possible only because of rule interference.

 
1. t1 by "foo", t2 by "foo"
   => fail "Tokens t1,t2 cannot be identified by the same literal 'foo'"

2. t1 by T, t2 by T
   => fail "Tokens t1,t2 cannot be identified by the same type T"

3. t by-nolit and t by "foo"
   => fail "Token t cannot be identified "

-------------------------------------

0)
Implicit literal: "foo"
Token is identified by "foo"
Token can be identified by any single type T

1)
"foo" T
Token can be identified by "foo" or type T

2)
"foo" object
Token can be identified by "foo"

3)
in single mode:
"foo" T1
"foo" T2
==>
Error scan pattern duplication

4)
"foo1" T
"foo2" T
==>
token is identified by type T
no token can be identified by "foo1" or "foo2"

5)
"foo" T
/bar/ T
==>
token is identified by type T
no token can be identified by "foo"

