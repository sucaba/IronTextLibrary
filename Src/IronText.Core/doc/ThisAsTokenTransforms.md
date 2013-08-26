1) prefixed & suffixed transform. Each usage of Items causes new set of rules (2 in following example).
Container : A1 Items B1
          | A2 Items B2
          ;

Items
    : /*empty*/
    | Items P1
    | Items P2
    | Items P3
    ;

== { ItemSyntax = A Items } ==>

// Transforms to two different situations:

ItemSyntax1  : A1; // begin method

ItemSyntax1  : ItemSyntax1 P1;
ItemSyntax1  : ItemSyntax1 P2;
ItemSyntax1  : ItemSyntax1 P3;

Container : ItemSyntax1 B1; // end method

ItemSyntax2  : A2; // begin method

ItemSyntax2  : ItemSyntax2 P1;
ItemSyntax2  : ItemSyntax2 P2;
ItemSyntax2  : ItemSyntax2 P3;

Container : ItemSyntax2 B2; // end method

2) sequence with prefix suffix is used as is. 

ThisAsToken interface is created from contextual rules.
This sometimes can cause ambiguities because of empty rule.
To avoid this problem, it is preferrable to follow transform #1 whenever possible
Alos #1 is oftem preferable because prefix & suffix tokens may have domain area 
meaning which makes #1 more readable for programmatic use (more readable DSL).


Items
    : /*empty*/
    | Items P1
    | Items P2
    | Items P3
    ;


3) Alternative (choice)

Choice : X | Y;

== { } ==>

ChoiceSyntax : /*empty*/ ;

Choice
    : ChoiceSyntax X
    | ChoiceSyntax Y
    ;

4) Optional token

Choice : /*empty*/ | X;

== { } ==>

MaybeSyntax : /*empty*/ ;

Choice
    : MaybeSyntax X  // end option
    | MaybeSyntax    // end option
    ;
