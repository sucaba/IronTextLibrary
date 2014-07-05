Feature: Find nullable, non-opt symbols to opt-symbols
	In order to get rid of empty productions (problematic for GLR parser runtime)
	As a language data provider
	I want to find nullable symbols


Scenario: Symbol with empty production and multiple non-empty producitons is converted into opt-symbol
	Given production 'X = term1'
	Given production 'X = term2'
	Given production 'X = '
	When find nullable non-opt symbols
    Then result symbols are 'X'

Scenario: Symbol with empty production and production with input size > 1 is converted into opt-symbol
	Given production 'X = term1 term2'
	Given production 'X = '
	When find nullable non-opt symbols
    Then result symbols are 'X'

Scenario: Symbol with single empty production is preserved as is
	Given production 'X = '
	When convert nullable symbols into opt
	When find nullable non-opt symbols
    Then result symbols are ''

Scenario: Opt-symbol production is preserved as is
	Given production 'X = term'
	Given production 'X = '
	When find nullable non-opt symbols
    Then result symbols are ''