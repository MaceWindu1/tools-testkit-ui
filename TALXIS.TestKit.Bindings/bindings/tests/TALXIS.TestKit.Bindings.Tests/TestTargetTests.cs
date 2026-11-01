namespace TALXIS.TestKit.Bindings.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reqnroll;
    using Reqnroll.Bindings.CucumberExpressions;
    using TALXIS.TestKit.Bindings.Configuration;

    /// <summary>
    /// The configured target and the steps that act on either kind of app.
    /// </summary>
    [TestClass]
    public class TestTargetTests
    {
        [TestMethod]
        public void ReadsTheTargetFromTheConfiguration()
        {
            Assert.AreEqual(TestTarget.Portal, Configure("Portal").Target);
            Assert.AreEqual(TestTarget.ModelDrivenApp, Configure("ModelDrivenApp").Target);
            Assert.IsNull(Configure(null).Target);
        }

        [DataTestMethod]
        [DataRow("Given", "I am logged in as 'a customer'", "GivenIAmLoggedInAs")]
        [DataRow("Given", "I am logged in to the portal as 'a customer'", "GivenIAmLoggedInToThePortalAs")]
        [DataRow("Given", "I am logged in to the 'Sales' app as 'a customer'", "GivenIAmLoggedInToTheAppAs")]
        [DataRow("When", "I open the 'Invoices' page from the navigation", "WhenIOpenThePageFromTheNavigation")]
        [DataRow("Then", "I should not see the 'Invoices' subarea", "ThenIShouldNotSeeTheSubarea")]
        [DataRow("Then", "I should be able to read 'an invoice'", "ThenIShouldBeAbleToRead")]
        public void MatchesOneBinding(string keyword, string text, string expectedMethod)
        {
            var matches = RegexBindings(keyword).Where(binding => binding.regex.IsMatch(text)).Select(binding => binding.method).ToList();

            CollectionAssert.AreEqual(new[] { expectedMethod }, matches);
        }

        private static TestConfiguration Configure(string? target)
        {
            var values = new Dictionary<string, string?> { ["Url"] = "https://example.test" };
            if (target != null)
            {
                values["Target"] = target;
            }

            return new TestConfiguration(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        }

        // The regular expression bindings of a step keyword, anchored as Reqnroll matches them.
        private static IEnumerable<(Regex regex, string method)> RegexBindings(string keyword)
        {
            var detector = new CucumberExpressionDetector();

            return typeof(PowerAppsStepDefiner).Assembly.GetTypes()
                .Where(type => type.IsDefined(typeof(BindingAttribute), inherit: true))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .SelectMany(method => method.GetCustomAttributes<StepDefinitionBaseAttribute>()
                    .Where(attribute => attribute.GetType().Name == keyword + "Attribute" && !detector.IsCucumberExpression(attribute.Regex))
                    .Select(attribute => (new Regex("^" + attribute.Regex.TrimStart('^').TrimEnd('$') + "$"), method.Name)));
        }
    }
}
