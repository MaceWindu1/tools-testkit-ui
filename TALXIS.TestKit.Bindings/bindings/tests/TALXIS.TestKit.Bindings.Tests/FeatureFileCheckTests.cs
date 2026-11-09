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

        private IReadOnlyList<string> Check(params string[] scenarioLines)
        {
            File.WriteAllLines(featureFile, new[] { "Feature: Checked" }.Concat(scenarioLines));
            return FeatureFileCheck.FindStepsWithoutOneBinding(new[] { featureFile }, new[] { typeof(CheckedSteps).Assembly });
        }

        [Binding]
        private sealed class CheckedSteps
        {
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
