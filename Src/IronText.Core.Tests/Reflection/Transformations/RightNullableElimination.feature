Feature: Right null elimination
	In order to build Reduction-Modified GLR-parser table
	As a language data provider
	I want to inline nullable suffixes in all productions.

Scenario: 1-time used empty symbol inlining
	Given production 'A = Y X'
    Given production 'X = '
	When eliminate right nullable symbols
	Then production exists 'A = Y (X = )'
    And  symbol 'X' is not used

Scenario: 1-time used opt-symbol inlining
	Given production 'A = Y X'
    Given production 'X = '
    Given production 'X = T'
	When eliminate right nullable symbols
	Then production exists 'A = Y (X = )'
	Then production exists 'A = Y (X = T)'
    And  symbol 'X' is not used

Scenario: 2-time used empty symbol inlining
	Given production 'A = Y X'
	Given production 'B = Z X'
    Given production 'X = '
	When eliminate right nullable symbols
	Then production exists 'A = Y (X = )'
	Then production exists 'B = Z (X = )'
    And  symbol 'X' is not used

Scenario: 2-time used nested-empty symbol inlining
	Given production 'A = Y X'
	Given production 'B = Z X'
    Given production 'X = T'
    Given production 'T = '
	When eliminate right nullable symbols
	Then production exists 'A = Y (X = (T = ))'
	Then production exists 'B = Z (X = (T = ))'
    And  symbol 'X' is not used

Scenario: 2-time used nested-opt symbol inlining
	Given production 'A = Y X'
	Given production 'B = Z X'
    Given production 'X = T'
    Given production 'T = '
    Given production 'T = t'
	When eliminate right nullable symbols
	Then production exists 'A = Y (X = (T = ))'
	Then production exists 'B = Z (X = (T = ))'
	Then production exists 'A = Y (X = (T = t))'
	Then production exists 'B = Z (X = (T = t))'
    And  symbol 'X' is not used

Scenario: Left-recursive symbol with empty production inlining
	Given production 'A = Y X'
    Given production 'X = '
    Given production 'X = X T'
	When eliminate right nullable symbols
    Then production exists 'A = Y (X = )'
    Then production exists 'A = Y (X = X T)'
     # Is used because of second production for 'A'
    Then symbol 'X' is used

Scenario: (-) Augmented production is not expanded
	Given production 'Start = '
	When eliminate right nullable symbols
	Then production exists '$start = Start'
    And  symbol 'Start' is used

Scenario: (-) 1-time used empty symbol is not inlined in a middle of production
	Given production 'A = Prefix X Suffix'
    Given production 'X = '
	When eliminate right nullable symbols
	Then production exists 'A = Prefix X Suffix'
    And  symbol 'X' is used

Scenario: (-) 1-time used empty symbol is not inlined at a beginning of production
	Given production 'A = X Suffix'
    Given production 'X = '
	When eliminate right nullable symbols
	Then production exists 'A = X Suffix'
    And  symbol 'X' is used
