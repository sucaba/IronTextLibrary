Feature: Recursive Eliminate Empty Productions
	In order to adapt grammar to parser runtime requirements
	As a language data provider
	I want to eliminate empty productions recursively

Scenario: Empty productions are eliminated recursively
	Given production 'X = '
	Given production 'Y = X'
    Given production 'Z = prefix Y suffix'
	When recursively eliminate empty productions
    Then production exists 'Z = prefix (Y = (X = )) suffix'
    Then 'Z' has 1 production
    Then symbol 'X' is not used
    Then symbol 'Y' is not used

Scenario: Multi-level null-symbols are eliminated recursively
	Given production 'X = '
	Given production 'Y = X'
	Given production 'Z = Y'
    Given production 'T = prefix X Z Y suffix'
	When recursively eliminate empty productions
    Then production exists 'T = prefix (X = ) (Z = (Y = (X = ))) (Y = (X = )) suffix'
    Then 'T' has 1 production
    Then symbol 'X' is not used
    Then symbol 'Y' is not used
    Then symbol 'Z' is not used

Scenario: Alternate nulls are eliminated recursively
	Given production 'X = '
	Given production 'Y = '
	Given production 'Y = X'
    Given production 'Z = prefix Y suffix'
	When recursively eliminate empty productions
    Then production exists 'Z = prefix (Y = (X = )) suffix'
    Then 'Z' has 1 production
    Then symbol 'X' is not used
    Then symbol 'Y' is not used
