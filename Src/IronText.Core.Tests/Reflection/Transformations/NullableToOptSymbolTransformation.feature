Feature: Nullable to optional transformation
	In order to get rid of empty productions
	As a language data provider
	I want to transform nullable symbols (symbols having direct empty production) in 
    gramamar into opt-symbols.

Scenario: Symbol with empty production and multiple non-empty producitons is converted into opt-symbol
	Given production 'X = term1'
	Given production 'X = term2'
	Given production 'X = '
	When convert nullable symbols into opt
	Then production exists 'X = Xnn'
	Then production exists 'X = '
	Then production exists 'Xnn = term1'
	Then production exists 'Xnn = term2'
    And 'X' has 2 productions
    And 'Xnn' has 2 productions

Scenario: Symbol with empty production and production with input size > 1 is converted into opt-symbol
	Given production 'X = term1 term2'
	Given production 'X = '
	When convert nullable symbols into opt
	Then production exists 'X = Xnn'
	Then production exists 'X = '
	Then production exists 'Xnn = term1 term2'
    And 'X' has 2 productions
    And 'Xnn' has 1 productions

Scenario: Symbol with single empty production is preserved as is
	Given production 'X = '
	When convert nullable symbols into opt
	Then production exists 'X = '
    Then 'X' has 1 productions

Scenario: Opt-symbol production is preserved as is
	Given production 'X = term'
	Given production 'X = '
	When convert nullable symbols into opt
	Then production exists 'X = '
	Then production exists 'X = term'
    Then 'X' has 2 productions