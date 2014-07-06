Feature: Resolution Of Production Duplicates
	In order to avoid duplicate productions
	As a language grammar provider
	I want to control how production duplicates are resolved upon production creation

Scenario: Create two productions with identical signature with 'Fail' resolver
    Given production duplicate resolver 'Fail'
	Given production 'X = term'
	When safe adding production 'X = term'
	Then result exception is 'System.InvalidOperationException'

Scenario: Create two productions with identical signature with 'IgnoreNew' resolver
    Given production duplicate resolver 'IgnoreNew'
	Given production 'X = term'
	When safe adding production 'X = term'
	Then no result exception caught
    Then 'X' has 1 production