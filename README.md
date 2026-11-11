# TALXIS TestKit Bindings

Reqnroll bindings library for Power Apps.
The aim of this project is to make Power Apps test automation easier, faster and more accessible. It does this by providing a library of generic Reqnroll step bindings that adhere to test automation best practices. This allows effective automated tests to be written without the dependency on developers who are both proficient with Power Apps and test automation.

## Thanks

A huge thank you to Capgemini and, in particular, to [Max Ewing](https://github.com/ewingjm) and his collaborators, as well as all contributors to [powerapps-specflow-bindings library](https://github.com/Capgemini/powerapps-specflow-bindings). With Max's permission and support, this library has been forked from this repository.

## Table of Contents

- [Installation](#Installation)
- [Usage](#Usage)
  - [Configuration](#Configuration)
  - [Writing feature files](#Writing-feature-files)
    - [Steps by app](#Steps-by-app)
  - [Writing step bindings](#Writing-step-bindings)
  - [Checking feature files](#Checking-feature-files)
  - [Test setup](#Test-setup)
    - [Bindings](#Bindings)
    - [Data file syntax](#Data-file-syntax)
    - [Dynamic data](#Dynamic-data)
- [Contributing](#Contributing)
- [Licence](#Licence)

## Installation

Follow the guidance in the **Installation and Setup** section in Reqnroll documentation [documentation](https://docs.reqnroll.net/latest/installation/setup-project.html). After installing the IDE integration and setting up your project, install the NuGet package.

```shell
PM> Install-Package TALXIS.TestKit.Bindings
```

Once the NuGet package is installed, follow the Reqnroll [documentation](https://docs.reqnroll.net/latest/installation/configuration.html) on referencing an external binding library. At the time of writing you should update a _reqnroll.json_ file in your project root as follows –

```json
{
    "stepAssemblies": [
        { "assembly": "TALXIS.TestKit.Bindings" }
    ]
}
```

### WebDrivers

This solution streamlines your test environment by automatically downloading and configuring the required WebDrivers for Chrome, Firefox, Edge, and IE.
Simply specify your desired browser in your configuration file, and the appropriate driver will be fetched and integrated at runtime.
This seamless approach eliminates the need for manual driver installations while ensuring that your tests always run with the latest version.

## Usage

### Configuration

After instalation TALXS.TestKit.Bindings you can configure it in your _appsetings.json_ file in your project's root. Now you can define the URL, browser, and users that will be used for your tests.

```json
{
  "url": "REQNROLL_POWERAPPS_URL",
  "useProfiles": false,
  "deleteTestData": true,
  "browserOptions": {
    "browserType": "Chrome",
    "headless": true,
    "width": 1920,
    "height": 1080,
    "startMaximized": false,
    "additionalCapabilities": {
      "capabilityName": "capabilityValue"
    }
  },
  "applicationUser": {
    "tenantId": "REQNROLL_POWERAPPS_TENANTID",
    "clientId": "REQNROLL_POWERAPPS_CLIENTID",
    "clientSecret": "REQNROLL_POWERAPPS_CLIENTSECRET"
  },
  "users": [
    {
      "username": "REQNROLL_POWERAPPS_USERNAME_SALESPERSON",
      "password": "REQNROLL_POWERAPPS_PASSWORD_SALESPERSON",
      "alias": "a salesperson",
      "otptoken": "REQNROLL_POWERAPPS_OTPTOKEN_SALESPERSON"
    }
  ]
}
```

The URL, usernames, passwords, and application user details can be overridden by user secrets or environment variables named by their configuration key (e.g. `Url` or `Users__0__Password`); the placeholder values above are not looked up. A URL that is not an absolute URL (e.g. a placeholder left in place) fails the first step that opens it with a message saying so. The browserOptions node supports anything in the TALXIS.TestKit.Selectors `BrowserOptions` class.

Tests will be distributed evenly between the users if multiple users are configured with the same alias. This can be helpful when you run a large number of tests in parallel and encounter errors due to user-level platform API limits.

#### User profiles

Setting the `useProfiles` property to true causes the solution to create and use a unique [profile](https://support.google.com/chrome/answer/2364824?co=GENIE.Platform%3DDesktop&hl=en) for each user listed in the config file. This currently only works in Chrome & Firefox; attempting to use it with Edge or IE will cause an exception to be thrown. By using profiles, test runs for the same user will not be required to re-authenticate, saving time during test runs. [To take full advantage of this you will need to have the "Stay Signed In" prompt enabled.](https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/keep-me-signed-in)

### Writing feature files

You can use the predefined step bindings to write your tests.

```gherkin
Scenario: User can create a new account
 Given I am logged in to the 'Sales Team Member' app as 'an admin'
 When I open the 'Accounts' sub area of the 'Customers' group
 Then I can see the 'New' command
```

Alternatively, write your own step bindings (see below).

#### Fields

- The field steps of a form take the field's label. The visibility and edit steps
  (`I should (not) be able to see the '<field>' field`, `I should (not) be able to edit the '<field>' field`) and
  `the '<field>' field should be empty` take its label or its logical name; in a model-driven app they look the label up
  in the form's controls, so they also find a hidden field by its label.
- `the '<field>' field should be empty` reads the form's value in a model-driven app, so it also checks a hidden field,
  e.g. one that a script cleared and hid.
- A multi-select option set takes the labels of its options separated by commas:
  `I enter 'Email, Phone' into the 'Preferred channels' field on the form`.
- Values entered into and expected in fields can be dates relative to the day of the run, as yyyy-MM-dd: `{{TODAY}}`,
  `{{TODAY:+n}}` or `{{TODAY:-n}}` (n days from today), e.g. `I enter '{{TODAY:-1}}' into the 'Valid From' field on the form`.

#### One feature file for a model-driven app and a TALXIS Portal

A TALXIS Portal shows the same forms, subgrids and commands as a model-driven app, so a feature file whose steps do not
name the kind of app can run against either. Set the kind of app at the URL with `target`:

```json
{
  "url": "REQNROLL_POWERAPPS_URL",
  "target": "Portal"
}
```

`target` is `ModelDrivenApp` (the default, so projects without it keep working) or `Portal`. The login, navigation,
tab, field, subgrid, command, dialog and record access steps act on the configured target;
[Steps by app](#Steps-by-app) lists the kinds of app each step acts on.

```gherkin
Scenario: A customer can read their invoices
 Given I am logged in as 'a customer'
 When I open the 'Invoices' page from the navigation
 Then I should be able to read 'an unpaid invoice'
```

- `I am logged in as '<alias>'` logs in to the target: the TALXIS Portal at the URL, or the model-driven app that the
  URL opens (by its `appid`). `I am logged in to the '<app>' app as '<alias>'` and
  `I am logged in to the portal as '<alias>'` keep logging in to a given kind of app.
- `I open the '<page>' page from the navigation` opens a navigation item by its label: a portal's navigation item, or a
  subarea of the app's current area.
- Steps for what only one kind of app has fail on the other with a `NotSupportedException` naming the operation, e.g. a
  disabled navigation item (`the '<item>' subarea should be disabled with the message '<message>'`), which only a portal
  shows.

#### TALXIS Portal

The navigation, tab, field, subgrid, command and dialog steps also work on a TALXIS Portal (signed in with
`I am logged in as '<alias>'` or `I am logged in to the portal as '<alias>'`, which signs in once more when Azure AD
B2C does not return to the portal, e.g. after its own error), with what the portal renders:

- Navigation items by their label or subarea id. A disabled item is shown with its message
  (`the '<item>' subarea should be disabled with the message '<message>'`); a hidden item is not rendered.
- Tabs by their label without the `#Icon#` prefix, or by their name. Fields by their label or logical name; while a
  form dialog is open, fields are looked up on the dialog. `I enter '<value>' into the '<field>' field on the form` (or
  `on the dialog`) sets text fields, option sets (by the option's label) and lookups; date fields are not supported yet.
- Subgrids by their control name. A command is found by its label, button id, command id, or, when it only shows an
  icon, by the icon's file name (`'download.svg'`). Commands for one selected record (an enable rule with a selection
  count of 1..1) are rendered on each record's row; the steps look for them on the rows of the selected records (all
  rows when none is selected), other commands on the subgrid's ribbon. The portal does not render a command whose
  rules fail, so a disabled command cannot be told from a missing one: `the '<command>' command on the '<subgrid>'
  subgrid should be disabled` fails with a `NotSupportedException`; check `I should not be able to see the '<command>'
  command on the '<subgrid>' subgrid` instead. In a subgrid without a selection column, selecting a
  record makes its row the one the command steps look at (in the legacy view it first clicks a plain cell of the row,
  which selects it; in the grid a click would open the record). To click a command on a record's row without
  selecting the record, as a user does (a grid can draw a selected row's highlight over its commands), use
  `I click the '<command>' command on '<alias>' in the '<subgrid>' subgrid`, or, by a displayed value,
  `I click the '<command>' command on the record with '<value>' in the '<column>' field in the '<subgrid>' subgrid`.
- A subgrid shown with the legacy view (e.g. in a nested form) has no record ids in the page; its records are found by
  a displayed value. In a grouped legacy view the grouped column's value is shown only in the group header: the steps
  expand the matching group and use its rows, or, when the group cannot be expanded (an aggregated row whose expander
  the page hides), the header, which then shows the commands.
- Row buttons of a dataset grid (notification expressions) by their title, aria-label or text, or, for an icon-only
  button that shows its label only in a tooltip, by its Fluent icon name (`'ClipboardList'`).
- The portal is ready once it has loaded its navigation bar and the page (`TALXIS.Portal.Context`), or when a dialog
  shown while it loads waits for input; a portal error dialog fails the step at once.
- Checks that something is not shown wait until it is gone and stays gone for a few seconds, since scripts hide
  navigation items and tabs after the page loads and grids refresh after a command. Selecting a tab clicks it again
  while it is not selected (a page can drop a click while it is loading), and closing a dialog waits for a dialog the
  previous step opened.
- A failed scenario saves a screenshot, the page's HTML and the browser's console (Chrome) in the `screenshots`
  folder next to the tests.
- Values of tab labels, dialog titles and field values can reference parameters (`{{Parameter:Name}}`, see below).
- A test record cannot be opened by its id: the portal shows a record in the form of the page that lists it, so
  `I have opened '<alias>'` fails with a `NotSupportedException`. Open the page and select the record in its subgrid.
- What the signed-in user may read: `I should be able to read '<alias>'` and `I should not be able to read '<alias>'`
  query a test record through the page's `Xrm.WebApi`, to which the portal applies its retrieve filters as it does to
  every page (in a model-driven app, the user's security roles apply). A record the query does not return is not
  readable; a failed query fails both steps.

#### Steps by app

The kinds of app each step acts on, worked out from the step bindings (a test keeps this table current):

- ✓: the step acts on that kind of app.
- ✓\*: the step acts on that kind of app, but some values fail with a `NotSupportedException` that names what is not
  supported (e.g. checking that a subgrid command is disabled on a TALXIS Portal).
- –: the step does not act on that kind of app: it is for the other kind of app only, fails on it with a
  `NotSupportedException`, or is not implemented.
- ?: not confirmed on that kind of app. The steps that act on no app (test data, record access, downloads) are listed by
  what runs against real apps confirmed.

<!-- BEGIN STEPS BY APP: generated by StepTargetTableTests -->

| Step | Model-driven app | TALXIS Portal |
| --- | :---: | :---: |
| `When I close the '(.*)' stage of the business process flow` | ✓ | – |
| `When I click next stage on the the '(.*)' stage of the business process flow` | ✓ | – |
| `When I pin the '(.*)' stage of the business process flow` | ✓ | – |
| `When I select the '(.*)' stage of the business process flow` | ✓ | – |
| `When I set the '(.*\|current)' stage of the business process flow as active` | ✓ | – |
| `When I enter '(.*)' into the '(.*)' field on the business process flow` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' (?:currency\|numeric\|text) field on the business process flow` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' optionset field on the business process flow` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' lookup field on the business process flow` | ✓ | – |
| `Then I should be able to see a value of '(true\|false)' in the '(.*)' boolean field on the business process flow` | ✓ | – |
| `Then I should be able to see a value of '((?:0?[1-9]\|[12][0-9]\|3[01])[\\/\\-](?:0?[1-9]\|1[012])[\\/\\-]\d{4}(?: \d{1,2}[:-]\d{2}(?:[:-]\d{2,3})*)?)' in the '(.*)' datetime field on the business process flow` | ✓ | – |
| `Then the record should be in the '(.*)' stage of the business process flow` | – | – |
| `When I select the '(.*)' command` | ✓ | – |
| `When I select the '([^']+)' command under the '([^']+)' flyout` | ✓ | – |
| `Then I should be able to see the '(.*)' command` | ✓ | – |
| `Then I should not be able to see the '(.*)' command` | ✓ | – |
| `When I click the Plus icon button in the Global Commands ribbon and select '(.*)' on the flyout` | ✓ | – |
| `When I select the '(.*)' command on the displayed dialog` | ✓ | ✓ |
| `When I click the '(.*)' button on the displayed dialog` | ✓ | ✓ |
| `When I choose the '(.*)' option on the dialog` | – | ✓ |
| `Then I should be able to see the '(.*)' button on the displayed dialog` | ✓ | ✓ |
| `Then I should not be able to see the '(.*)' button on the displayed dialog` | ✓ | ✓ |
| `When I close the displayed dialog without saving` | ✓ | ✓ |
| `When I click the '(.*)' button on the displayed dialog repeatedly until it is no longer available` | ✓ | ✓ |
| `When I click the '(.*)' button on the displayed dialog repeatedly until '(.*)' is visible` | ✓ | ✓ |
| `When I enter the following into the dialog` | ✓\* | ✓ |
| `When I enter '(.*)' into the '(.*)' field on the dialog` | ✓\* | ✓ |
| `Then a dialog (should\|should not) be displayed` | ✓ | ✓ |
| `Then a dialog with '(.*)' title should be displayed` | ✓ | ✓ |
| `Then the values of the fields on the dialog should be as the following` | ✓ | ✓ |
| `Then the '(.*)' field on the dialog should show the error '(.*)'` | – | ✓ |
| `Then I should be able to see a value of '(.*)' in the '(.*)' field on the dialog` | ✓ | ✓ |
| `When I select the '(.*)' dashboard` | ✓ | – |
| `Given I have opened '(.*)'` | ✓ | – |
| `Given I have created '(.*)'` | ✓ | ✓ |
| `Given '(.*)' exists` | ✓ | ✓ |
| `Given I have created the following records` | ✓ | ✓ |
| `Given the following records exist` | ✓ | ✓ |
| `Given '(.*)' has created '(.*)'` | ? | ? |
| `When I (confirm\|cancel) when presented with the confirmation dialog` | ✓ | ✓ |
| `When I assign to me on the assign dialog` | ✓ | – |
| `When I assign to a (user\|team) named '(.*)' on the assign dialog` | ✓ | – |
| `When I close the opportunity as (won\|lost)` | ✓ | – |
| `When I close the alert dialog` | ✓ | ✓ |
| `When I click (confirm\|cancel) on the publish dialog` | ✓ | – |
| `When I click (ok\|cancel) on the set state dialog` | ✓ | – |
| `Then an alert dialog should be displayed` | ✓ | ✓ |
| `Then an alert dialog with title '(.*)' should be displayed` | ✓ | ✓ |
| `Then a confirmation dialog with title '(.*)' should be displayed` | ✓ | ✓ |
| `When I select the '(.*)' tab` | ✓ | ✓ |
| `Then I (should\|should not) be able to see the '(.*)' tab` | ✓ | ✓ |
| `When I enter '(.*)' into the '(.*)' field on the form` | ✓ | ✓ |
| `When I enter the following into the form` | ✓ | ✓ |
| `When I clear the '(.*)' (?:currency\|numeric\|text\|boolean) field` | ✓ | – |
| `When I clear the '(.*)' optionset field` | ✓ | – |
| `When I clear the '(.*)' multioptionset field` | ✓ | – |
| `When I clear the '(.*)' datetime field` | ✓ | – |
| `When I clear the '(.*)' lookup field` | ✓ | – |
| `When I delete the record` | ✓ | – |
| `When I open the record set navigator` | ✓ | – |
| `When I close the record set navigator` | ✓ | – |
| `When I select '(.*)' form` | ✓ | – |
| `When I select '(.*)' lookup` | ✓ | – |
| `When I save the record` | ✓ | – |
| `When I assign the record to a (?:user\|team) named '(.*)'` | ✓ | – |
| `When I switch process to the '(.*)' process` | ✓ | – |
| `Then the values of the fields should be as the following` | ✓ | ✓ |
| `Then I should be able to see a value of '(.*)' in the '(.*)' field` | ✓ | ✓ |
| `Then the '(.*)' field should be (mandatory\|optional)` | ✓ | ✓ |
| `Then I should be able to see a value of '(true\|false)' in the '(.*)' boolean (field\|header field)` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' datetime (field\|header field)` | ✓ | – |
| `Then I should be able to see a business process error stating '(.*)'` | ✓ | – |
| `Then I should be presented with a '(.*)' form for the '(.*)' entity` | ✓ | – |
| `Then I should be able to see an info form notification stating '(.*)'` | ✓ | – |
| `Then I should be able to see a warning form notification stating '(.*)'` | ✓ | – |
| `Then I should be able to see an error form notification stating '(.*)'` | ✓ | – |
| `Then I should be able to see a value of '(.*)' as the header title` | ✓ | – |
| `Then I should be able to edit the '(.*)' field` | ✓ | ✓ |
| `Then I should be able to see the following options in the '(.*)' option set field` | ✓ | – |
| `Then I should be able to edit the following fields` | ✓ | ✓ |
| `Then I should not be able to edit the '(.*)' field` | ✓ | ✓ |
| `Then I should not be able to edit the following fields` | ✓ | ✓ |
| `Then I should be able to see the '(.*)' field` | ✓ | ✓ |
| `Then I should not be able to see the '(.*)' field` | ✓ | ✓ |
| `Then the '(.*)' field should be empty` | ✓ | ✓ |
| `Then the status of the record should be (active\|inactive)` | ✓ | – |
| `Then I should be able to see the '(.*)' section` | ✓ | – |
| `Then I should not be able to see the '(.*)' section` | ✓ | – |
| `Then the active tab should be '(.*)'` | ✓ | ✓ |
| `When I click the '(.*)' command on the '(.*)' subgrid` | ✓ | ✓ |
| `When I click the '(.*)' command on '(.*)' in the '(.*)' subgrid` | – | ✓ |
| `When I click the '(.*)' command on the record with '(.*)' in the '(.*)' field in the '(.*)' subgrid` | – | ✓ |
| `When I select '(.*)' from the '(.*)' subgrid` | ✓ | ✓ |
| `When I select all in the '(.*)' subgrid` | ✓ | – |
| `When I open the record at position '(\d+)' in the '(.*)' subgrid` | ✓ | – |
| `When I open the (\d+(?:(?:st)\|(?:nd)\|(?:rd)\|(?:th))) record in the '(.*)' subgrid` | ✓ | – |
| `When I search for '(.*)' in the '(.*)' subgrid` | ✓ | – |
| `When I switch to the '(.*)' view in the '(.*)' subgrid` | ✓ | – |
| `Then I should not be able to see '(.*)' in the '(.*)' subgrid` | ✓ | ✓ |
| `Then I should be able to see (exactly\|more than\|less than) (\d+) record in the '(.*)' subgrid` | ✓ | ✓ |
| `Then I should be able to see (exactly\|more than\|less than) (\d+) records in the '(.*)' subgrid` | ✓ | ✓ |
| `Then the '(.*)' subgrid should contain '(.*)'` | ✓ | ✓ |
| `Then the '(.*)' subgrid should contain the following records` | ✓ | ✓ |
| `Then the '(.*)' subgrid should contain a record with a reference to '(.*)' in the '(.*)' lookup field` | ✓ | – |
| `Then the '(.*)' subgrid should contain records with the following in the '(.*)' lookup` | ✓ | – |
| `Then the '(.*)' subgrid should contain a record with '(.*)' in the '(.*)' (?:text\|numeric\|currency\|lookup\|datetime\|optionset\|multioptionset\|boolean) field` | ✓ | ✓ |
| `Then I should be able to see the '(.*)' command on the '(.*)' subgrid` | ✓ | ✓ |
| `When I click the '([^']+)' flyout on the '([^']+)' subgrid` | ✓ | – |
| `Then I should not be able to see the '(.*)' command on the '(.*)' subgrid` | ✓ | ✓ |
| `Then the '(.*)' command on the '(.*)' subgrid should be enabled` | ✓ | ✓ |
| `Then the '(.*)' command on the '(.*)' subgrid should be disabled` | ✓ | – |
| `Then I should be able to see the '(.*)' command on the flyout of the subgrid` | ✓ | – |
| `Then I should not be able to see the '(.*)' command on the flyout of the subgrid` | ✓ | – |
| `When I click the '([^']+)' command under the '([^']+)' flyout on the '([^']+)' subgrid` | ✓ | – |
| `When I select a record with '(.*)' in the '(.*)' field in the '(.*)' subgrid` | ✓ | ✓ |
| `Then a file should have been downloaded` | ✓ | ✓ |
| `When I apply a search filter using the filter '(.*)' with the filter value of '(.*)' group` | ✓ | – |
| `When I apply a search filter using the filter '(.*)'` | ✓ | – |
| `When I search globally using the filter '(.*)'` | ✓ | – |
| `When I open a record from global search on the entity '(.*)' in the position of '(.*)'` | ✓ | – |
| `When I open the record at position '(\d+)' in the grid` | ✓ | – |
| `When I open the (\d+(?:(?:st)\|(?:nd)\|(?:rd)\|(?:th))) record in the grid` | ✓ | – |
| `When I clear the search in the grid` | ✓ | – |
| `When I sort the '(.*)' column in the grid using the '(.*)' option` | ✓ | – |
| `When I switch to the '(.*)' view in the grid` | ✓ | – |
| `When I select '(.*)' from the grid` | ✓ | – |
| `Then the grid should contain '(.*)'` | ✓ | – |
| `When I select the following records from the grid` | ✓ | – |
| `When I search for '(.*)' in the grid` | ✓ | – |
| `Given I am logged in to the '(.*)' app as '(.*)'` | ✓ | – |
| `Given I am logged in as '(.*)'` | ✓ | ✓ |
| `When I select '([^']+)' in the lookup dialog` | ✓ | – |
| `When I click Add in the lookup dialog` | ✓ | – |
| `When I click the new button in the lookup` | ✓ | – |
| `When I open the record at position '(\d+)' in the lookup` | ✓ | – |
| `When I open the (\d+(?:(?:st)\|(?:nd)\|(?:rd)\|(?:th))) record in the lookup` | ✓ | – |
| `When I search for '(.*)' in the '(.*)' lookup` | ✓ | – |
| `When I select the related '(.*)' entity in the lookup` | ✓ | – |
| `When I switch to the '(.*)' view in the lookup` | ✓ | – |
| `Then I should see only the following records in the '(.*)' lookup` | ✓ | – |
| `When I select a related '(.*)' lookup field` | ✓ | – |
| `When I click to perform an advanced lookup on '(.*)' lookup` | ✓ | – |
| `When I open the sub area '(.*)' under the '(.*)' area` | ✓ | – |
| `When I open global search` | ✓ | – |
| `When I open the '(.*)' area` | ✓ | – |
| `When I open the '(.*)' sub area of the '(.*)' group` | ✓ | – |
| `When I open a quick create for the '(.*)' entity` | ✓ | – |
| `When I sign out` | ✓ | – |
| `Then I should see the '(.*)' area` | ✓ | – |
| `Then I should see the '(.*)' subarea` | ✓ | ✓ |
| `Then I should see the '(.*)' group` | ✓ | – |
| `When I open the '(.*)' page from the navigation` | ✓ | ✓ |
| `Then I should not see the '(.*)' subarea` | ✓ | ✓ |
| `Then the '(.*)' subarea should be disabled with the message '(.*)'` | – | ✓ |
| `Given I am logged in to the portal as '(.*)'` | – | ✓ |
| `When I enter '(.*)' into the '(.*)' field on the quick create` | ✓ | – |
| `When I enter the following into the quick create` | ✓ | – |
| `When I cancel the quick create` | ✓ | – |
| `When I clear the '(.*)' (?:currency\|numeric\|text\|datetime\|boolean) field on the quick create` | ✓ | – |
| `When I clear the '(.*)' optionset field on the quick create` | ✓ | – |
| `When I clear the '(.*)' lookup field on the quick create` | ✓ | – |
| `When I save the quick create` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' (?:currency\|numeric\|text) field on the quick create` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' multioptionset field on the quick create` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' optionset field on the quick create` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' lookup field on the quick create` | ✓ | – |
| `Then I should be able to see a value of '(true\|false)' in the '(.*)' boolean field on the quick create` | ✓ | – |
| `Then I should be able to see a value of '(.*)' in the '(.*)' datetime field on the quick create` | ✓ | – |
| `Then I should be able to read '(.*)'` | ✓ | ✓ |
| `Then I should not be able to read '(.*)'` | ✓ | ✓ |
| `When I open the record at position '(\d+)' in the related grid` | ✓ | – |
| `When I open the (\d+(?:(?:st)\|(?:nd)\|(?:rd)\|(?:th))) record in the related grid` | ✓ | – |
| `When I open the related '(.*)' tab` | ✓ | – |
| `When I click the '(.*)' button on the related grid` | ✓ | – |
| `Then I should see a '(.*)' button in the flyout on the related grid` | ✓ | – |
| `Then I should not see a '(.*)' button in the flyout on the related grid` | ✓ | – |
| `When I add an appointment to the timeline with the subject '(.*)', the description '(.*)', the duration '(.*)', and the location '(.*)'` | ✓ | – |
| `When I add a note to the timeline with the title '(.*)' and the body '(.*)'` | ✓ | – |
| `When I add a phone call to the timeline with the subject '(.*)', the description '(.*)', the duration '(.*)', and the number '(\d*)'` | ✓ | – |
| `When I post '(.*)' to the timeline` | ✓ | – |
| `When I add a task to the timeline with the subject '(.*)', the description '(.*)' and the duration '(.*)'` | ✓ | – |
| `When I add an email to the timeline with the subject '(.*)', the duration '(.*)', and the following contacts` | ✓ | – |
| `When I click create for (?:a\|an) '(.*)' on the timeline` | ✓ | – |
| `When I wait up to '(.*)' seconds` | ✓ | – |
| `Then the virtual dataset grid should contain '(.*)'` | – | ✓ |
| `When I click the '(.*)' button on '(.*)' in the virtual dataset grid` | – | ✓ |
| `Then I should be able to see the '(.*)' button on '(.*)' in the virtual dataset grid` | – | ✓ |
| `Then I should not be able to see the '(.*)' button on '(.*)' in the virtual dataset grid` | – | ✓ |

<!-- END STEPS BY APP -->

### Writing step bindings

You can write your own step bindings that have thread-safe access to EasyRepro and the Selenium WebDriver. This ensures that your tests can be run safely in parallel. You can do this by creating a class decorated with the reqnroll `Binding` attribute and inheriting from the `PowerAppsStepDefiner` base class. You can then create your reqnroll step bindings by interacting with the `XrmApp` and `Driver` properties.

```csharp
[Binding]
public class MyCustomSteps : PowerAppsStepDefiner
{
    [Given(@"I have a custom step")]
    public void GivenIHaveACustomStep()
    {
      // Interact with the inherited EasyRepro 'Browser' object.
      // Interact with the inherited Selenium 'Driver' object.
    }
}
```

A step that should work on a model-driven app and on a TALXIS Portal alike uses the `App` property instead: an
`IAppUnderTest` (TALXIS.TestKit.Selectors) for the configured target, with the operations a user performs (log in, open
a page, select a tab, set a field, click a subgrid command, answer a dialog) rather than the markup. Both apps are in
TALXIS.TestKit.Selectors: `ModelDrivenApp` acts through `XrmApp`, and `PortalApp`, the portal's counterpart of
`XrmApp`, keeps the portal's selectors (`PortalElements`) apart from the model-driven app's (`AppElements`). Steps for
one kind of app only use the `ModelDrivenApp` or `PortalApp` property. An operation the target does not have throws
`NotSupportedException`.

```csharp
[Then(@"the '(.*)' field should be (mandatory|optional)")]
public static void ThenTheFieldShouldBeMandatory(string field, string requirementLevel)
{
    App.Entity.IsFieldRequired(field, requirementLevel == "mandatory").Should().BeTrue();
}
```

### Checking feature files

Reqnroll matches a step only to a binding of its keyword, and `And` and `But` take the keyword of the step before them:
after `Given I have opened 'a record'`, `And I select the 'Details' tab` does not match the
`When I select the '(.*)' tab` binding. By default, Reqnroll does not fail a scenario with a step that matches no
binding; it reports the scenario as pending (skipped or inconclusive in the test results), so a test run can look
green while such scenarios never ran. Make them fail in _reqnroll.json_:

```json
{
  "runtime": {
    "missingOrPendingStepsOutcome": "Error"
  }
}
```

To find these steps without a browser or an environment, before a test run, add a test that checks the feature files
with `FeatureFileCheck`. It matches each step as Reqnroll does (by its keyword, with each Examples row's values, to the
binding's regular expression, Cucumber expression or method name, and to a binding whose method takes the step's
arguments, including a data table or doc string) and lists each step that matches no binding or more than one, the
binding a step would match with another keyword, and each binding whose expression Reqnroll cannot read. Scoped
bindings (`[Scope]`) are matched as unscoped ones.

```csharp
[TestMethod]
public void EveryStepMatchesOneBinding()
{
    var featureFiles = Directory.EnumerateFiles(FeatureFolder, "*.feature", SearchOption.AllDirectories);

    var problems = FeatureFileCheck.FindStepsWithoutOneBinding(
        featureFiles,
        new[] { typeof(MyCustomSteps).Assembly, typeof(PowerAppsStepDefiner).Assembly });

    Assert.AreEqual(0, problems.Count, string.Join(Environment.NewLine, problems));
}
```

`FeatureFolder` is the folder of your feature files (e.g. the project folder, found by going up from
`AppContext.BaseDirectory`); the check fails when it is given no feature files. Pass the assemblies with your step
bindings and those that _reqnroll.json_ lists, and, when _reqnroll.json_ sets another `language.feature`, that language
(`featureLanguage: "de"`).

Reqnroll runs its `[BeforeTestRun]` hooks before any test of a Reqnroll test project, this one too: with `useProfiles`,
the users sign in first. To run the check alone without signing in, set the `UseProfiles` environment variable to
`false`, e.g. `$env:UseProfiles = 'false'; dotnet test --filter EveryStepMatchesOneBinding`.

### Test setup

#### Bindings

We avoid performing test setup via the UI. This speeds up test execution and makes the tests more robust (as UI automation is more fragile than using supported APIs). _Given_ steps should therefore be carried out using the [Client API](client-api), [WebAPI](web-api) or [Organization Service](org-service).

You can create test data by using the following _Given_ steps –

```gherkin
Given I have created 'a record'
```

or

```gherkin
Given 'someone' has created 'a record with a difference'
```

These bindings look for a corresponding JSON file in a _data_ folder in the root of your project. The file is resolved using a combination of the directory structure and the file name. For example, the bindings above could resolve the following files:

```shell
└───data
    │   a record.json
    │
    └───a record
            with a difference.json
```

The `deleteTestData` property in the appsetings.json file can be set to specify whether you want records created via these bindings to be deleted after a scenario has run. You may wish to override the default value and retain these (e.g. to aid in diagnosing failures).

If you are using the binding which creates data as someone other than the current user, you will need the following configuration to be present:

- A user with a matching alias in the `users` array that has the `username` set.
- An application user with sufficient privileges to impersonate the above user configured in the `applicationUser` property.

Refer to the Microsoft documentation on creating an application user [here](https://docs.microsoft.com/en-us/power-platform/admin/create-users-assign-online-security-roles#create-an-application-user).

#### Data file syntax

The JSON is similar to that expected by Web API when creating records via a [deep insert](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/webapi/create-entity-web-api#create-related-entities-in-one-operation).

```json
{
  "@logicalName": "account",
  "@alias": "sample account",
  "name": "Sample Account",
  "primarycontactid": {
    "firstname": "John",
    "lastname": "Smith"
  },
  "opportunity_customer_accounts": [
    {
      "name": "Opportunity associated to Sample Account",
      "Opportunity_Tasks": [{ "subject": "Task associated to opportunity" }]
    }
  ]
}
```

The example above will create the following:

- An account
- A primary contact related to the account
- An opportunity related to the account
- A task related to the opportunity

In addition to the standard Web API syntax, we also have the following:

| Property     | Description                                                                                                                                                  | Requirement                                              |
|--------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------|
| @logicalName | the entity logical name of the root record                                                                                                                   | Mandatory (unless included using `@extends` – see below) |
| @alias       | a friendly alias that can be used to reference the created record in certain bindings. Can be set on nested records                                          | Optional                                                 |
| @extends     | a relative path to a data file to extend. Records in arrays are merged by index (you may need to include blank objects to insert new records into the array) | Optional                                                 |

Dataverse creates every record active. A record with an inactive `statecode` (and its `statuscode`) is created active and
its state is set once it and its related records exist. When a plugin or process gives an active record another status
reason as it is created, the record's `statuscode` from the data file is set again.

#### Dynamic data

We support [faker.js](https://github.com/marak/Faker.js) moustache template syntax for generating dynamic test data at run-time. Please refer to the faker documentation for all of the functionality that is available. The below JSON will generate a contact with a random name, credit limit, email address, and date of birth in the past 90 years:

```json
{
  "@logicalName": "contact",
  "@alias": "a dynamically generated contact",
  "lastname": "{{name.firstName}}",
  "firstname": "{{name.lastName}}",
  "creditlimit@faker.number": "{{finance.amount}}",
  "emailaddress1": "{{internet.email}}",
  "birthdate@faker.date": "{{date.past(90)}}",
  "accountid@alias.bind": "sample account"
}
```

When using faker syntax, you must also annotate number or date fields using `@faker.number`, `@faker.date` or `@faker.dateonly` to ensure that the JSON is formatted correctly. Text fields need no annotation, e.g. `"name": "Tower {{datatype.uuid}}"` gives each run a record with its own name.

You can also dynamically set lookups by alias using `<lookup>@alias.bind` (this is limited to aliased records in other files – not the current file).

#### Parameters

Data files and the values entered by field steps (e.g. `I enter '...' into the '...' field`) can reference named values from the `parameters` section of the configuration as `{{Parameter:Name}}`. Use them for identifiers that differ between runners or environments, such as the existing records a test runs against, so that each runner can use its own records with the same feature and data files:

```json
"parameters": {
  "TestAccountId": "11111111-2222-3333-4444-555555555555",
  "TestAccountNumber": "ACC-0001"
}
```

```json
{
  "@logicalName": "contact",
  "lastname": "Contact of {{Parameter:TestAccountNumber}}",
  "parentcustomerid_account@odata.bind": "/accounts({{Parameter:TestAccountId}})"
}
```

Values are inserted as they are, so in data files they must not contain characters that need escaping in JSON. Override them per runner like any other setting, e.g. with user secrets (`Parameters:TestAccountId`) or environment variables (`Parameters__TestAccountId`). A missing parameter fails the step. Custom step bindings can resolve their arguments the same way with `ReplaceParameters(TestConfig.Parameters)`.

#### Record in specific Business Process Flow stage

In case you need a record in a specific BPF (Business Process Flow) stage, you can specify the `@bpf` parameter in your data file which needs a logical name of the target BPF and an id of the stage. Example:

```json
{
  "@extends": "../an opportunity",
  "@logicalName": "talxis_opportunityheader",
  "@alias": "the opportunity in won stage",
  "@bpf": {
    "@logicalName": "talxis_opportunitybusinessprocessflow",
    "@activestageid": "b573544e-8c04-4b6d-ac3d-84d810d92ac1"
  }
}
```

You can use WebApi to get the stage id you need where _processid_value is equal to the id of your BPF, like this:

```csharp
await Xrm.WebApi.retrieveMultipleRecords("processstage", "?$filter=_processid_value eq BA1AF566-5105-4E24-8B75-0A7F01A24079");
```

## Contributing

Please refer to the [Contributing](./CONTRIBUTING.md) guide.

The TALXIS Portal extensions (finding navigation items, tabs, fields, subgrid rows and commands, dialogs) have tests in
`TALXIS.TestKit.Bindings/bindings/tests/TALXIS.TestKit.Bindings.Tests`. They run in a headless Chrome against
`TestPages/portal.html`, a static copy of the DOM the portal renders; when the portal renders something differently,
reproduce it on the page first. The driver's data handling has Karma specs in `TALXIS.TestKit.Bindings/driver/test`.

```shell
dotnet test TALXIS.TestKit.Bindings/bindings/tests/TALXIS.TestKit.Bindings.Tests
```

## Licence

TALXIS TestKit Bindings is released under the [MIT licence](./LICENCE).