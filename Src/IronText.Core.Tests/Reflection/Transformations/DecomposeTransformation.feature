Feature: Decompose Grammar Transformation
    In order to inline productions of some kind
	As a grammar transformation algorith writer
	I want to decompose productions of some non-terminal into the other, newly-created non-terminal

Scenario: Extract non-terminal using 'contains symbol' criteria
    Given production 'X = D'
	And production 'X = E'
	And production 'X = D E'
	And production 'X = '
	And production criteria is: input has 'E' symbol
	When decompose symbol 'Xnew' from symbol 'X'
	Then result symbol is 'Xnew'
    And production exists 'Xnew = E'
    And production exists 'Xnew = D E'
    And 'Xnew' has 2 productions
    And production exists 'X = D'
    And production exists 'X = '
    And production exists 'X = Xnew'
    And 'X' has 3 productions

Scenario: Extract non-terminal using 'is non-empty' criteria
    Given production 'X = D'
	And production 'X = D E'
	And production 'X = '
	And production criteria is: input is not empty
	When decompose symbol 'Xnew' from symbol 'X'
	Then result symbol is 'Xnew'
    And production exists 'Xnew = D'
    And production exists 'Xnew = D E'
    And 'Xnew' has 2 productions
    And production exists 'X = '
    And production exists 'X = Xnew'
    And 'X' has 2 productions