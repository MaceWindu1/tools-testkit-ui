namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Selectors;
    using TALXIS.TestKit.Selectors.Browser;

    /// <summary>
    /// Extensions to the <see cref="Field"/> class.
    /// </summary>
    public static class FieldExtensions
    {
        /// <summary>
        /// This is required as the <see cref="Field.IsReadOnly"/> property does not work.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <returns>Whether the field is read-only.</returns>
        [Obsolete("Use Entity.IsFieldReadOnly(fieldName).")]
        public static bool IsReadOnly(this Field field, IWebDriver driver)
        {
            field = field ?? throw new ArgumentNullException(nameof(field));

            driver = driver ?? throw new ArgumentNullException(nameof(driver));

            ReadOnlyCollection<IWebElement> lockedIcons = driver.FindElements(
                By.XPath(AppElements.Xpath[AppReference.Entity.FieldContainer].Replace("[NAME]", (field.Name + "-locked-iconWrapper").ToXPathLiteral())));

            return lockedIcons.Any();
        }
    }
}
