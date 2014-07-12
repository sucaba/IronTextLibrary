Feature: Find nullable non-opt symbols
	In order to get rid of empty productions (problematic for GLR parser runtime)
	As a language data provider
	I want to find nullable symbols

Scenario: Symbol with empty production and multiple non-empty producitons is in result
	Given production 'X = term1'
	Given production 'X = term2'
	Given production 'X = '
	When find nullable non-opt symbols
    Then result symbols are 'X'

Scenario: Symbol with empty production and production with input size > 1 is in result
	Given production 'X = term1 term2'
	Given production 'X = '
	When find nullable non-opt symbols
    Then result symbols are 'X'

Scenario: Symbol with single empty production is not in result
	Given production 'X = '
	When find nullable non-opt symbols
    Then result symbols are ''

Scenario: Opt-symbol is not in result
	Given production 'X = term'
	Given production 'X = '
	When find nullable non-opt symbols
    Then result symbols are ''