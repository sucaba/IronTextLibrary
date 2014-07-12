Feature: Eliminate Empty Productions
	In order to adapt grammar to parser runtime requirements
	As a language data provider
	I want to eliminate empty productions

Scenario: Eliminate null-symbols
	Given production 'X = '
	Given production 'Y = prefix X suffix'
	When eliminate empty productions
	Then production exists 'Y = prefix (X = ) suffix'
    Then symbol 'X' is not used 
    Then 'Y' has 1 productions 

Scenario: Eliminate nullable opt-symbol
	Given production 'X = '
	Given production 'X = term'
	Given production 'Y = prefix X suffix'
	When eliminate empty productions
	Then production exists 'Y = prefix (X = ) suffix'
	Then production exists 'Y = prefix (X = term) suffix'
    Then 'Y' has 2 productions
    Then symbol 'X' is not used 

Scenario: Eliminate nullable symbol having multiple non-null productions
	Given production 'X = '
	Given production 'X = term1'
	Given production 'X = term2'
	Given production 'Y = prefix X suffix'
	When eliminate empty productions
	Then production exists 'Y = prefix (X = ) suffix'
	Then production exists 'Y = prefix (X = term1) suffix'
	Then production exists 'Y = prefix (X = term2) suffix'
    Then 'Y' has 3 productions 
    Then symbol 'X' is not used 

Scenario: Eliminate nullable symbol having single multi-symbol productions
	Given production 'X = '
	Given production 'X = term1 term2 term3'
	Given production 'Y = prefix X suffix'
	When eliminate empty productions
	Then production exists 'Y = prefix (X = ) suffix'
	Then production exists 'Y = prefix (X = term1 term2 term3) suffix'
    Then 'Y' has 2 productions 
    Then symbol 'X' is not used 

Scenario: Eliminate null tails
	Given production 'X = '
	Given production 'Y = prefix X'
	When eliminate empty productions
	Then production exists 'Y = prefix (X = )'
    Then symbol 'X' is not used 
    Then 'Y' has 1 productions 

Scenario: Eliminate null heads
	Given production 'X = '
	Given production 'Y = X suffix'
	When eliminate empty productions
	Then production exists 'Y = (X = ) suffix'
    Then symbol 'X' is not used 
    Then 'Y' has 1 productions 

Scenario: Empty productions are not eliminated recursively
	Given production 'X = '
	Given production 'Y = X'
    Given production 'Z = Y'
	When eliminate empty productions
	Then production exists 'Y = (X = )'
	Then production exists 'Z = Y'
    Then symbol 'X' is not used 
    Then 'Y' has 1 productions 
    Then 'Z' has 1 productions 
