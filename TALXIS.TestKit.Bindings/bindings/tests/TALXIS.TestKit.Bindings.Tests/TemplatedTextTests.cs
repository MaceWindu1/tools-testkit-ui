namespace TALXIS.TestKit.Bindings.Tests
{
    using System;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TALXIS.TestKit.Bindings.Extensions;
    using TALXIS.TestKit.Bindings.Transformations;

    [TestClass]
    public class TemplatedTextTests
    {
        [TestMethod]
        public void ReplacesTodayAndDaysFromIt()
        {
            Assert.AreEqual(Date(0), "{{TODAY}}".ReplaceTemplatedText());
            Assert.AreEqual(Date(-1), "{{TODAY:-1}}".ReplaceTemplatedText());
            Assert.AreEqual(Date(180), "{{TODAY:+180}}".ReplaceTemplatedText());
        }

        [TestMethod]
        public void ReplacesEachOfSeveralTemplates()
        {
            Assert.AreEqual($"from {Date(-1)} to {Date(1)}", "from {{TODAY:-1}} to {{TODAY:+1}}".ReplaceTemplatedText());
        }

        [TestMethod]
        public void ReplacesARandomNumberWithinItsBounds()
        {
            Assert.AreEqual("Unit 7", "Unit {{RANDNUM:7,8}}".ReplaceTemplatedText());
        }

        [TestMethod]
        public void TransformsARelativeDateForTheDateSteps()
        {
            Assert.AreEqual(DateTime.Today.AddDays(-1), UtilityTransformations.TransformRelativeDate("{{TODAY:-1}}"));
        }

        [TestMethod]
        public void RefusesAnUnknownTemplate()
        {
            Assert.ThrowsException<NotImplementedException>(() => "{{YESTERDAY}}".ReplaceTemplatedText());
        }

        private static string Date(int days) => DateTime.Today.AddDays(days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
