namespace TALXIS.TestKit.Bindings.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extensions to the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        private const string DateFormat = "yyyy-MM-dd";

        private static readonly Regex ParameterPattern = new Regex(@"{{\s*Parameter\s*:\s*([^}]+?)\s*}}", RegexOptions.IgnoreCase);

        private static readonly Regex TemplatePattern = new Regex("{{(.*?)}}");

        /// <summary>
        /// Replaces <c>{{Parameter:Name}}</c> placeholders with configured values and leaves any other text unchanged.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="parameters">The configured parameters.</param>
        /// <returns>The resulting string.</returns>
        public static string ReplaceParameters(this string input, IReadOnlyDictionary<string, string> parameters)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));

            return ParameterPattern.Replace(input, match =>
            {
                string name = match.Groups[1].Value;
                if (parameters == null || !parameters.TryGetValue(name, out string value))
                {
                    throw new InvalidOperationException($"The parameter '{name}' is not configured. Add it to the 'parameters' section of the test configuration.");
                }

                return value;
            });
        }

        /// <summary>
        /// Looks for templated text in string and replaces: <c>{{Parameter:Name}}</c>, <c>{{RANDNUM:min,max}}</c> and
        /// <c>{{TODAY}}</c>, <c>{{TODAY:+n}}</c> or <c>{{TODAY:-n}}</c> (today's date, or n days from it, as yyyy-MM-dd).
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="parameters">The configured parameters for <c>{{Parameter:Name}}</c> placeholders.</param>
        /// <returns>The resulting string.</returns>
        public static string ReplaceTemplatedText(this string input, IReadOnlyDictionary<string, string> parameters = null)
        {
            input = input?.ReplaceParameters(parameters) ?? throw new ArgumentNullException(nameof(input));

            var random = new Random();

            return TemplatePattern.Replace(input, match =>
            {
                string[] templateSplit = match.Groups[1].Value.Split(':');

                switch (templateSplit[0].ToUpperInvariant())
                {
                    case "RANDNUM":
                        string[] randomBound = templateSplit[1].Split(',');
                        int lowerBound = int.Parse(randomBound[0], CultureInfo.CurrentCulture);
                        int upperBound = int.Parse(randomBound[1], CultureInfo.CurrentCulture);

                        return random.Next(lowerBound, upperBound).ToString(CultureInfo.InvariantCulture);
                    case "TODAY":
                        int days = templateSplit.Length > 1 ? int.Parse(templateSplit[1], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) : 0;

                        return DateTime.Today.AddDays(days).ToString(DateFormat, CultureInfo.InvariantCulture);
                    default:
                        throw new NotImplementedException($"ReplaceTemplatedText does not support {templateSplit[0]}.");
                }
            });
        }
    }
}
