Feature: Find nullable symbols to opt-symbols
	In order to get rid of empty productions (problematic for GLR parser runtime)
	As a language data provider
	I want to find nullable symbols

Scenario: Symbol with empty production is nullable
	Given production 'X = T'
	Given production 'X = '
	Given production 'X = Y Z'
	When find nullable symbols
    Then result symbols are 'X'

Scenario: Symbol with empty production and recursive production is nullable
	Given production 'X = X Z'
	Given production 'X = '
	Given production 'X = T'
	When find nullable symbols
    Then result symbols are 'X'

Scenario: Symbol with nullable-identity production is nullable
	Given production 'X = N'
	Given production 'X = Y'
	Given production 'Y = N2'
	Given production 'Y = '
	When find nullable symbols
    Then result symbols are 'X Y'

Scenario: Symbol with production from multiple nullable symbols is nullable
	Given production 'X = NonEmpty'
	Given production 'X = Y Z T'
	Given production 'Y = term2'
	Given production 'Y = '
	Given production 'Z = Y T'
	Given production 'T = '
	Given production 'NonEmpty = term1'
	When find nullable symbols
    Then result symbols are 'X Y Z T'
