Feature: DetectRecursiveSymbols
	In order to avoid stack overflow when inlining recursive symbols
	As a language data provider
	I want to detect if symbol is recursive

Scenario: Left recursive symbol is detected
	Given production 'X = X term'
	Given production 'X = exit'
	When detect if symbol 'X' is recursive
	Then result should be 'true'

Scenario: Right recursive symbol is detected
	Given production 'X = term X'
	Given production 'X = exit'
	When detect if symbol 'X' is recursive
	Then result should be 'true'

Scenario: Inner recursive symbol is detected
	Given production 'X = prefix X suffix'
	Given production 'X = '
	When detect if symbol 'X' is recursive
	Then result should be 'true'

Scenario: Nested recursive symbol is detected
	Given production 'X = prefix Y suffix'
	Given production 'X = exit'
	Given production 'Y = prefix2 X suffix2'
	Given production 'Y = exit2'
	When detect if symbol 'X' is recursive
	Then result should be 'true'

Scenario: (-) Symbol non-recursively referencing recursive symbol is not detected
	Given production 'X = prefix Y suffix'
	Given production 'Y = prefix2 Y suffix2'
	Given production 'Y = exit2'
	When detect if symbol 'X' is recursive
	Then result should be 'false'

Scenario: (-) Terminal is not detected
	Given symbol 'term'
	When detect if symbol 'term' is recursive
	Then result should be 'false'