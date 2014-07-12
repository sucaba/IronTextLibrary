Feature: Global Symbol Inlining
	In order to get rid of productions unneccessary for parser runtime
	As a language data provider
	I want to to inline some non-terminals symbols

Scenario: Nonterminal symbol with single empty production is inlined
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = '
	When inline grammar
	Then production exists 'S = Prefix (X = ) Suffix'
    And 'X' has 0 productions
    And 'S' has 1 productions

Scenario: Nonterminal symbol with nested empty production is inlined
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = Y'
	And production 'Y = '
	When inline grammar
	Then production exists 'S = Prefix (X = (Y = )) Suffix'
    And 'X' has 0 productions
    And 'Y' has 0 productions
    And 'S' has 1 productions

Scenario: Nonterminal symbol with 1-terminal production is inlined
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = t'
	When inline grammar
	Then production exists 'S = Prefix (X = t) Suffix'
    And 'X' has 0 productions
    And 'S' has 1 productions

Scenario: Nonterminal symbol with 3-terminal production is inlined
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = t s p'
	When inline grammar
	Then production exists 'S = Prefix (X = t s p) Suffix'
    And 'X' has 0 productions
    And 'S' has 1 productions

Scenario: Nonterminal symbol with nested 3-terminal production is inlined
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = Y'
	And production 'Y = t s p'
	When inline grammar
	Then production exists 'S = Prefix (X = (Y = t s p)) Suffix'
    And 'X' has 0 productions
    And 'Y' has 0 productions
    And 'S' has 1 productions