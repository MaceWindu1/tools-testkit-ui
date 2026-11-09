namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reqnroll;
    using TALXIS.TestKit.Bindings.Authoring;

    [TestClass]
    public class FeatureFileCheckTests
    {
        private string featureFile = null!;

        [TestInitialize]
        public void CreateFeatureFile()
        {
            featureFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.feature");
        }

        [TestCleanup]
        public void DeleteFeatureFile()
        {
            File.Delete(featureFile);
        }

        [TestMethod]
        public void TheSampleFeatureMatchesTheTestKitBindings()
        {
            var sample = Path.Combine(AppContext.BaseDirectory, "Samples", "EntitySteps.feature");

            var problems = FeatureFileCheck.FindStepsWithoutOneBinding(new[] { sample }, new[] { typeof(PowerAppsStepDefiner).Assembly });

            Assert.AreEqual(0, problems.Count, string.Join(Environment.NewLine, problems));
        }

        [TestMethod]
        public void ReportsAStepWithoutABinding()
        {
            var problems = Check(
                "Scenario: Unbound step",
                "  Given a value of 'x'",
                "  When nothing is bound to this");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:4: no binding for 'nothing is bound to this'" }, problems.ToArray());
        }

        [TestMethod]
        public void ReportsAStepWithSeveralBindings()
        {
            var problems = Check(
                "Scenario: Ambiguous step",
                "  When an ambiguous step");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:3: 'an ambiguous step' matches CheckedSteps.WhenAnAmbiguousStep, CheckedSteps.WhenTheSameAmbiguousStep" }, problems.ToArray());
        }

        [TestMethod]
        public void ChecksEachExamplesRowOfAScenarioOutline()
        {
            var problems = Check(
                "Scenario Outline: Outcomes",
                "  Then the outcome is '<outcome>'",
                "Examples:",
                "  | outcome |",
                "  | ok      |",
                "  | wrong   |",
                "  | fine    |");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:3: no binding for 'the outcome is 'wrong''" }, problems.ToArray());
        }

        [TestMethod]
        public void MatchesAndAndButStepsByThePreviousKeyword()
        {
            var problems = Check(
                "Scenario: Keywords",
                "  Given a value of 'x'",
                "  And a value of 'y'",
                "  Then the outcome is 'ok'",
                "  But the outcome is 'fine'",
                "  And a value of 'z'");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:7: no Then binding for 'a value of 'z''; it matches CheckedSteps.GivenAValueOf (Given)" }, problems.ToArray());
        }

        [TestMethod]
        public void MatchesCucumberExpressions()
        {
            var problems = Check(
                "Scenario: Cucumber expression",
                "  Given 3 items are in stock",
                "  Given three items are in stock");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:4: no binding for 'three items are in stock'" }, problems.ToArray());
        }

        [TestMethod]
        public void ReportsABackgroundStepOnce()
        {
            var problems = Check(
                "Background:",
                "  Given nothing is bound to this",
                "Scenario: First",
                "  Given a value of 'x'",
                "Rule: A rule",
                "  Scenario: Second",
                "    Given a value of 'y'");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:3: no binding for 'nothing is bound to this'" }, problems.ToArray());
        }

        [TestMethod]
        public void MatchesABindingWithoutAnExpressionByItsMethodName()
        {
            var problems = Check(
                "Scenario: Method name",
                "  When I save the record");

            Assert.AreEqual(0, problems.Count, string.Join(Environment.NewLine, problems));
        }

        [TestMethod]
        public void KnowsTheParameterTypesOfTransformationsAndEnums()
        {
            var problems = Check(
                "Scenario: Parameter types",
                "  Given the contact John Smith is selected",
                "  Then the color is Red");

            Assert.AreEqual(0, problems.Count, string.Join(Environment.NewLine, problems));
        }

        [TestMethod]
        public void ReportsAStepWithoutTheArgumentsOfItsBinding()
        {
            var problems = Check(
                "Scenario: Arguments",
                "  When I enter the following");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:3: 'I enter the following' gives 0 argument(s), but CheckedSteps.WhenIEnterTheFollowing takes 1" }, problems.ToArray());
        }

        [TestMethod]
        public void MatchesTheBindingThatTakesTheArgumentsOfTheStep()
        {
            var problems = Check(
                "Scenario: Arguments",
                "  Then the list is shown",
                "  Then the list is shown",
                "    | name |",
                "    | A    |");

            Assert.AreEqual(0, problems.Count, string.Join(Environment.NewLine, problems));
        }

        [TestMethod]
        public void MatchesTheStepMethodsThatABindingInherits()
        {
            var problems = Check(
                "Scenario: Inherited",
                "  Given an inherited step");

            Assert.AreEqual(0, problems.Count, string.Join(Environment.NewLine, problems));
        }

        [TestMethod]
        public void RefusesToCheckNoFeatureFiles()
        {
            Assert.ThrowsException<ArgumentException>(() => FeatureFileCheck.FindStepsWithoutOneBinding(Array.Empty<string>(), new[] { typeof(CheckedSteps).Assembly }));
        }

        [TestMethod]
        public void NamesTheFeatureFileThatCannotBeParsed()
        {
            File.WriteAllLines(featureFile, new[] { "Given a step without a feature" });

            var error = Assert.ThrowsException<FormatException>(() => FeatureFileCheck.FindStepsWithoutOneBinding(new[] { featureFile }, new[] { typeof(CheckedSteps).Assembly }));

            StringAssert.StartsWith(error.Message, featureFile);
        }

        [TestMethod]
        public void ParsesFeatureFilesInTheGivenLanguage()
        {
            File.WriteAllLines(featureFile, new[] { "Funktionalität: Geprüft", "Szenario: Deutsch", "  Angenommen a value of 'x'", "  Wenn nothing is bound to this" });

            var problems = FeatureFileCheck.FindStepsWithoutOneBinding(new[] { featureFile }, new[] { typeof(CheckedSteps).Assembly }, featureLanguage: "de");

            CollectionAssert.AreEqual(new[] { $"{featureFile}:4: no binding for 'nothing is bound to this'" }, problems.ToArray());
        }

        private IReadOnlyList<string> Check(params string[] scenarioLines)
        {
            File.WriteAllLines(featureFile, new[] { "Feature: Checked" }.Concat(scenarioLines));
            return FeatureFileCheck.FindStepsWithoutOneBinding(new[] { featureFile }, new[] { typeof(CheckedSteps).Assembly });
        }

        // Step methods of a base class without [Binding], which Reqnroll matches for the binding classes that inherit them.
        private abstract class InheritedSteps
        {
            [Given("^an inherited step$")]
            public void GivenAnInheritedStep()
            {
            }
        }

        private enum Color
        {
            Red,
        }

        private sealed record Contact(string Name);

        [Binding]
        private sealed class CheckedSteps : InheritedSteps
        {
            [StepArgumentTransformation]
            public static Contact ToContact(string name) => new(name);

            [When]
            public void When_I_save_the_record()
            {
            }

            [Given("the contact {Contact} is selected")]
            public void GivenTheContactIsSelected(Contact contact)
            {
            }

            [Then("the color is {Color}")]
            public void ThenTheColorIs(Color color)
            {
            }

            [When("^I enter the following$")]
            public void WhenIEnterTheFollowing(Table values)
            {
            }

            [Then("^the list is shown$")]
            public void ThenTheListIsShown()
            {
            }

            [Then("^the list is shown$")]
            public void ThenTheListIsShownWith(Table rows)
            {
            }

            [Given("^a value of '(.*)'$")]
            public void GivenAValueOf(string value)
            {
            }

            [Then("^the outcome is '(ok|fine)'$")]
            public void ThenTheOutcomeIs(string outcome)
            {
            }

            [When("^an ambiguous step$")]
            public void WhenAnAmbiguousStep()
            {
            }

            [When("^an ambiguous step$")]
            public void WhenTheSameAmbiguousStep()
            {
            }

            [Given("{int} items are in stock")]
            public void GivenItemsAreInStock(int count)
            {
            }
        }
    }
}
