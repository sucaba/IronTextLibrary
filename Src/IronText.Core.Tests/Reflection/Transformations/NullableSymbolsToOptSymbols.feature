Feature: Nullable symbols to opt-symbols
	In order to get rid of empty productions (problematic for GLR parser runtime)
	As a math idiot
	I want to transform nullable symbols into opt-symbols (later are inlinable by other algorithms)

Scenario: Single non-recursive nullable symbol is transformed into opt-symbol
    Given production 'S = X'
	Given production 'X = '
	Given production 'X = Y Z'
	Given production 'X = T'
	When nullable to opt symbols
    Then symbol exists 'Xnn'
	Then production exists 'X = '
	Then production exists 'X = Xnn'
	Then production exists 'Xnn = Y Z'
	Then production exists 'Xnn = T'