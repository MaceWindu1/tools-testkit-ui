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
        private static readonly Regex ParameterPattern = new Regex(@"{{\s*Parameter\s*:\s*([^}]+?)\s*}}", RegexOptions.IgnoreCase);

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
        /// Looks for templated text in string and replaces.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="parameters">The configured parameters for <c>{{Parameter:Name}}</c> placeholders.</param>
        /// <returns>The resulting string.</returns>
        public static string ReplaceTemplatedText(this string input, IReadOnlyDictionary<string, string> parameters = null)
        {
            input = input?.ReplaceParameters(parameters) ?? throw new ArgumentNullException(nameof(input));

            MatchCollection matches = Regex.Matches(input, "{{.*}}");
            var random = new Random();

            foreach (Match match in matches)
            {
                string innerText = match.Value.Substring(2, match.Value.Length - 4);
                string[] templateSplit = innerText.Split(':');

                if (templateSplit[0].ToUpperInvariant() == "RANDNUM")
                {
                    string[] randomBound = templateSplit[1].Split(',');
                    int lowerBound = int.Parse(randomBound[0], CultureInfo.CurrentCulture);
                    int upperBound = int.Parse(randomBound[1], CultureInfo.CurrentCulture);

                    input = input
                        .Remove(
                            match.Index,
                            match.Length)
                        .Insert(
                            match.Index,
                            random
                                .Next(lowerBound, upperBound)
                                .ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new NotImplementedException($"ReplaceTemplatedText does not support {templateSplit[0]}.");
                }
            }

            return input;
        }
    }
}
