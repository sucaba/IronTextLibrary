Feature: Opt-symbol inlining
	In order to get rid of empty productions (problematic for GLR parser runtime)
	As a language data provider
	I want to to inline non-terminal symbols matching opt-symbol pattern

Scenario: Opt-symbol pattern is detected
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = '
	And production 'X = Y'
	And production 'Y = t'
	And production 'Z = '
	And production 'Z = t'
	When find optional pattern symbols
    Then result symbols are 'X Z'

Scenario: Symbol with empty production is not identified as optional pattern
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = '
	When find optional pattern symbols
    Then result symbols are ''

Scenario: Symbol with identity production is not identified as optional pattern
	Given used symbol 'S'
    And production 'S = Prefix X Suffix'
	And production 'X = t'
	When find optional pattern symbols
    Then result symbols are ''

Scenario: Optional-terminal pattern is inlined
    Given used symbol 'S'
    And production 'S = Prefix X Y Suffix'
    And production 'X = '
    And production 'X = t1'
    And production 'Y = '
    And production 'Y = T'
    And production 'T = t2'
    When inline optional symbols 
    Then production exists 'S = Prefix Suffix'
    And production exists 'S = Prefix t1 Suffix'
    And production exists 'S = Prefix T Suffix'
    And production exists 'S = Prefix t1 T Suffix'
    And 'X' has 0 productions
    And 'Y' has 0 productions
    And 'S' has 4 productions
    And 'T' has 1 productions