namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using OpenQA.Selenium;
    using TALXIS.TestKit.Selectors;

    /// <summary>
    /// Extensions to the <see cref="Entity"/> class.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// This is required as <see cref="Entity.GetField"/> throws a <see cref="NullReferenceException"/> when getting a hidden field.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>Whether the field is visible.</returns>
        public static bool IsFieldVisible(this Entity entity, IWebDriver driver, string fieldName)
        {
            driver = driver ?? throw new ArgumentNullException(nameof(driver));

            var elements = driver.FindElements(By.CssSelector($"div[data-id={fieldName}]"));

            return elements.Count > 0;
        }

        /// <summary>
        /// Gets the logical name of a field on the form from its logical name or its label. Unlike
        /// <see cref="GetFieldLogicalNameFromLabel"/>, which reads the labels shown, it reads the form's controls, so it also
        /// finds hidden fields. A name that matches neither is returned unchanged.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="field">The logical name or the label of the field.</param>
        /// <returns>Logical name of the field.</returns>
        public static string ResolveFieldLogicalName(this Entity entity, IWebDriver driver, string field)
        {
            driver = driver ?? throw new ArgumentNullException(nameof(driver));

            return (string)((IJavaScriptExecutor)driver).ExecuteScript(
                @"var field = arguments[0];
                  if (Xrm.Page.getAttribute(field)) { return field; }
                  var labelled = Xrm.Page.ui.controls.get(function (control) {
                      return typeof control.getLabel === 'function' && control.getLabel() === field
                          && typeof control.getAttribute === 'function' && control.getAttribute();
                  });
                  return labelled.length > 0 ? labelled[0].getAttribute().getName() : field;",
                field);
        }

        /// <summary>
        /// Determines whether a field on the form has no value, whether it is shown or hidden.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="fieldName">The logical name of the field.</param>
        /// <returns>Whether the field is empty.</returns>
        public static bool IsFieldEmpty(this Entity entity, IWebDriver driver, string fieldName)
        {
            driver = driver ?? throw new ArgumentNullException(nameof(driver));

            var empty = ((IJavaScriptExecutor)driver).ExecuteScript(
                @"var attribute = Xrm.Page.getAttribute(arguments[0]);
                  if (!attribute) { return null; }
                  var value = attribute.getValue();
                  return value === null || value === '' || (Array.isArray(value) && value.length === 0);",
                fieldName);

            return empty as bool? ?? throw new InvalidOperationException($"The form has no '{fieldName}' field.");
        }

        /// <summary>
        /// Gets logical name of a field on form from its label.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="fieldLabel">The label of the field.</param>
        /// <returns>Logical name of the field.</returns>
        public static string GetFieldLogicalNameFromLabel(this Entity entity, IWebDriver driver, string fieldLabel)
        {
            driver = driver ?? throw new ArgumentNullException(nameof(driver));

            var labelElement = driver.FindElement(By.XPath($"//label[text()='{fieldLabel}']/ancestor-or-self::div[@data-preview_orientation='row'][1]"));
            string logicalName = labelElement.GetAttribute("data-id");

            return logicalName;
        }

        /// <summary>
        /// Determines if the section on the form is visible.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="sectionName">The name of the section.</param>
        /// <returns>Whether the section is visible.</returns>
        public static bool IsSectionVisible(this Entity entity, IWebDriver driver, string sectionName)
        {
            driver = driver ?? throw new ArgumentNullException(nameof(driver));

            var elements = driver.FindElements(By.CssSelector($"*[data-id={sectionName}]"));

            return elements.Count > 0;
        }
    }
}
