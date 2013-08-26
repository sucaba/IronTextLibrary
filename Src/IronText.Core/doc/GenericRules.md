Generic Rules
-------------

In many cases PLs and DSLs have rules with same structure but vary in participating tokens.
Example:

// List of Identifier
IdentifierList ==> /*Empty*/
IdentifierList ==> IdentifierList Identifier

// List of Statement
StatementList : /*Empty*/;
StatementList : StatementList Statement;

It looks redundant to specify such recurring patterns over and over again.
From other point of view this problems is very similar to the c# generic methods and types.

In Wasp it will be solved as following rules in builtin language module:

[Pattern]
List<T> EmptyList<T>() { return new List<T>(); }
[Pattern]
List<T> List<T>(List<T> items, T item) { items.Add(item); return items; }

This pattern-methods do not cause direct grammer rule generation but instead they act as a rule factories.
Each time framework sees new token type matching left-hand side it will automatically instantiate existing
rule templates.