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
        [Obsolete("Use Entity.IsFieldVisible(fieldName).")]
        public static bool IsFieldVisible(this Entity entity, IWebDriver driver, string fieldName)
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            return entity.IsFieldVisible(fieldName);
        }

        /// <summary>
        /// Gets logical name of a field on form from its label.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="fieldLabel">The label of the field.</param>
        /// <returns>Logical name of the field.</returns>
        [Obsolete("Use Entity.GetFieldLogicalNameFromLabel(fieldLabel).")]
        public static string GetFieldLogicalNameFromLabel(this Entity entity, IWebDriver driver, string fieldLabel)
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            return entity.GetFieldLogicalNameFromLabel(fieldLabel);
        }

        /// <summary>
        /// Determines if the section on the form is visible.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="driver">The Selenium WebDriver.</param>
        /// <param name="sectionName">The name of the section.</param>
        /// <returns>Whether the section is visible.</returns>
        [Obsolete("Use Entity.IsSectionVisible(sectionName).")]
        public static bool IsSectionVisible(this Entity entity, IWebDriver driver, string sectionName)
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));
            return entity.IsSectionVisible(sectionName);
        }
    }
}
