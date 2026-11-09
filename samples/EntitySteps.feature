Feature: Entity Steps

	In order to automate entity interaction
	As a developer
	I want to use pre-existing entity bindings

Background:
	Given I am logged in to the 'Mock App' app as 'an admin'
	When I open the sub area 'Mock Records' under the 'Primary Group' area
	When I select the 'New' command
	And I select 'Information' form

Scenario: Select a tab
	When I select the 'Secondary' tab

Scenario: Assert tab is visible
	Then I should be able to see the 'Primary' tab

Scenario: Assert tab is hidden
	Then I should not be able to see the 'Hidden' tab

Scenario Outline: Enter values on a form
	When I enter '<value>' into the '<columnLabel>' field on the form
	Then I should be able to see a value of '<value>' in the '<columnLabel>' field

Scenarios:
		| columnLabel   | value              |
		| Text          | Some text          |
		| Number        | 10                 |
		| Yes No        | true               |
		| Choice        | Option A           |
		| Choices       | Option A, Option B |
		| Date And Time | 1/1/2021 13:00     |
		| Date Only     | 1/1/2021           |
		| Currency      | £10.00             |
		| Lookup        | Record Name        |

Scenario: Enter lookup on a form
	Given I have created 'a secondary mock record'
	When I enter 'A secondary mock record' into the 'Lookup' field on the form
	Then I should be able to see a value of 'A secondary mock record' in the 'Lookup' field

Scenario: Enter multiple values on a form
	When I enter the following into the form
		| Value     | Field  |
		| Some text | Text   |
		| 10        | Number |

Scenario: Clear lookup value
	Given I have created 'a secondary mock record'
	When I enter 'A secondary mock record' into the 'Lookup' field on the form
	And I clear the 'Lookup' lookup field
	Then I should be able to see a value of '' in the 'Lookup' field

Scenario: Delete a record
	Given I have created 'a secondary mock record'
	And I have opened 'the secondary mock record'
	When I delete the record

Scenario: Open and close the record set navigator
	Given I have created 'data decorated with faker moustache syntax'
	And I have created 'a record with an alias'
	When I open the sub area 'Mock Records' under the 'Primary Group' area
	And I open the record at position '0' in the grid
	And I open the record set navigator
	And I close the record set navigator

Scenario: Select a lookup
	When I select 'Lookup' lookup

Scenario: Save a record
	When I enter 'Some text' into the 'Name' field on the form
	And I save the record

Scenario: Assign to a user or team
	Given I have created 'a team'
	And I have created 'a secondary mock record'
	And I have opened 'the secondary mock record'
	When I assign the record to a team named 'A team'

Scenario: Switch process
	Given I have created 'a record with a business process flow'
	And I have opened 'the record'
	When I switch process to the 'Secondary Business Process Flow' process

Scenario: Assert a field is optional
	Then the 'Text' field should be optional

Scenario: Assert a field is required
	Then the 'Name' field should be mandatory

Scenario Outline: Assert fom notification message
	Then I should be able to see <type> form notification stating '<text>'

Scenarios:
		| type      | text                             |
		| an info   | A mock info form notification    |
		| a warning | A mock warning form notification |
		| an error  | A mock error form notification   |

Scenario: Assert header title
	Then I should be able to see a value of 'New Mock Record' as the header title

Scenario Outline: Assert field editable
	Then I should be able to edit the '<columnLabel>' field

Scenarios:
		| columnLabel   |
		| Text          |
		| Number        |
		| Yes No        |
		| Choice        |
		| Choices       |
		| Date And Time |
		| Date Only     |
		| Currency      |

Scenario: Assert fields editable
	Then I should be able to edit the following fields
		| Field         |
		| Text          |
		| Number        |
		| Yes No        |
		| Choice        |
		| Choices       |
		| Date And Time |
		| Date Only     |
		| Currency      |

Scenario: Assert option set options
	Then I should be able to see the following options in the 'Choice' option set field
		| Option   |
		| Option A |
		| Option B |
		| Option C |

Scenario: Assert field visible
	Then I should be able to see the 'Name' field

Scenario: Assert field not visible
	Then I should not be able to see the 'Owner' field

Scenario: Assert record status
	Then the status of the record should be active

Scenario: Assert business process error message
	When I enter 'Some text' into the 'Name' field on the form
	And I enter 'true' into the 'sb_triggerbusinessprocesserror' field on the form
	And I save the record
	Then I should be able to see a business process error stating 'Mock business process error'

Scenario: Assert field not editable
	Then I should not be able to edit the 'Created On' field

Scenario: Assert fields not editable
	Then I should not be able to edit the following fields
		| Field      |
		| Created On |
		| Created By |