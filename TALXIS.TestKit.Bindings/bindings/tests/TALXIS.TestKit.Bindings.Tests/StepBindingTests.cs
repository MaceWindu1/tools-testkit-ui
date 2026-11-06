namespace TALXIS.TestKit.Bindings.Tests
{
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reqnroll;
    using Reqnroll.Bindings.CucumberExpressions;

    [TestClass]
    public class StepBindingTests
    {
        /// <summary>
        /// Reqnroll reads an expression without '^' or '$' and without the regex syntax it recognizes (such as '(.*)') as a
        /// Cucumber expression, in which '(confirm|cancel)' is optional literal text: such a step never matches.
        /// </summary>
        [TestMethod]
        public void NoBindingIsReadAsACucumberExpressionWithARegexChoice()
        {
            var detector = new CucumberExpressionDetector();

            var misread = typeof(PowerAppsStepDefiner).Assembly.GetTypes()
                .Where(type => type.IsDefined(typeof(BindingAttribute), inherit: true))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .SelectMany(method => method.GetCustomAttributes<StepDefinitionBaseAttribute>().Select(attribute => (method, expression: attribute.Regex)))
                .Where(binding => binding.expression.Contains('|') && detector.IsCucumberExpression(binding.expression))
                .Select(binding => $"{binding.method.DeclaringType!.Name}.{binding.method.Name}: {binding.expression}")
                .ToList();

            Assert.AreEqual(0, misread.Count, "Anchor these expressions with '^' and '$':\n" + string.Join("\n", misread));
        }
    }
}
