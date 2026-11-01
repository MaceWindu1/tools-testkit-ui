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
  - [Writing step bindings](#Writing-step-bindings)
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

The URL, usernames, passwords, and application user details will be set from environment variables (if found). Otherwise, the value from the config file will be used. The browserOptions node supports anything in the TALXIS.TestKit.Selectors `BrowserOptions` class.

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

#### One feature file for a model-driven app and a TALXIS Portal

A TALXIS Portal shows the same forms, subgrids and commands as a model-driven app, so a feature file whose steps do not
name the kind of app can run against either. Set the kind of app at the URL with `target`:

```json
{
  "url": "REQNROLL_POWERAPPS_URL",
  "target": "Portal"
}
```

`target` is `ModelDrivenApp` or `Portal`. The navigation, tab, field, subgrid, command, dialog and record access steps
act on the configured target. Without `target` they tell the two apart by the open page, and
`I am logged in as '<alias>'` cannot be used.

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
- Steps for what only one kind of app has fail on the other with a `NotSupportedException`, e.g. a disabled navigation
  item (`the '<item>' subarea should be disabled with the message '<message>'`), which only a portal shows.

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
  rules fail, so a disabled command is one that is not shown. In a subgrid without a selection column, selecting a
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
- Checks that something is not shown wait until it is gone and stays gone for a few seconds, since scripts hide
  navigation items and tabs after the page loads and grids refresh after a command. Selecting a tab clicks it again
  while it is not selected (a page can drop a click while it is loading), and closing a dialog waits for a dialog the
  previous step opened.
- A failed scenario saves a screenshot, the page's HTML and the browser's console (Chrome) in the `screenshots`
  folder next to the tests.
- Values of tab labels, dialog titles and field values can reference parameters (`{{Parameter:Name}}`, see below).
- What the signed-in user may read: `I should be able to read '<alias>'` and `I should not be able to read '<alias>'`
  query a test record through the page's `Xrm.WebApi`, to which the portal applies its retrieve filters as it does to
  every page (in a model-driven app, the user's security roles apply). A record the query does not return is not
  readable; a failed query fails both steps.

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

When using faker syntax, you must also annotate number or date fields using `@faker.number`, `@faker.date` or `@faker.dateonly` to ensure that the JSON is formatted correctly.

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