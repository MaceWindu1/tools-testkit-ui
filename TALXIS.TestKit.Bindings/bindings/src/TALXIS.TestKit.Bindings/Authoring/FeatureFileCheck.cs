namespace TALXIS.TestKit.Bindings.Authoring
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Gherkin;
    using Gherkin.Ast;
    using Reqnroll;
    using Reqnroll.Bindings;
    using Reqnroll.Bindings.CucumberExpressions;

    /// <summary>
    /// Checks feature files against the step bindings without a browser or an environment. A step that matches no binding
    /// or several otherwise only fails when its scenario runs, possibly late in a long test run.
    /// </summary>
    public static class FeatureFileCheck
    {
        private static readonly Regex Placeholder = new Regex("<([^<>]+)>");
        private static readonly IReadOnlyDictionary<string, string> NoExamplesRow = new Dictionary<string, string>();

        /// <summary>
        /// Finds the steps that match no step binding or more than one. The steps are matched as Reqnroll matches them: by
        /// their keyword ('And' and 'But' take the previous step's), with the values of each Examples row filled in, and
        /// against the binding's regular expression or Cucumber expression. Scoped bindings are matched as unscoped ones.
        /// </summary>
        /// <param name="featureFiles">The paths of the feature files.</param>
        /// <param name="bindingAssemblies">The assemblies with step bindings: the test project's and those that reqnroll.json
        /// lists (such as TALXIS.TestKit.Bindings).</param>
        /// <returns>One line per such step: where it is, its text and the bindings it matches.</returns>
        public static IReadOnlyList<string> FindStepsWithoutOneBinding(IEnumerable<string> featureFiles, IEnumerable<Assembly> bindingAssemblies)
        {
            if (featureFiles == null)
            {
                throw new ArgumentNullException(nameof(featureFiles));
            }

            if (bindingAssemblies == null)
            {
                throw new ArgumentNullException(nameof(bindingAssemblies));
            }

            var bindings = LoadBindings(bindingAssemblies);
            return featureFiles
                .SelectMany(StepsOf)
                .Select(step => (step, matches: bindings.Where(binding => binding.Matches(step)).Select(binding => binding.Method).Distinct().ToList()))
                .Where(check => check.matches.Count != 1)
                .Select(check => check.matches.Count == 0
                    ? NoBindingFor(check.step, bindings)
                    : $"{check.step.Location}: '{check.step.Text}' matches {string.Join(", ", check.matches.Select(NameOf))}")
                .Distinct()
                .ToList();
        }

        // Reqnroll matches a step only to a binding of its keyword: 'Given ... And I select the tab' does not run a
        // [When] binding of 'I select the tab'.
        private static string NoBindingFor(ScenarioStep step, IReadOnlyList<StepBinding> bindings)
        {
            var otherKeywords = bindings.Where(binding => binding.Regex.IsMatch(step.Text)).ToList();
            return otherKeywords.Count == 0
                ? $"{step.Location}: no binding for '{step.Text}'"
                : $"{step.Location}: no {step.Type} binding for '{step.Text}'; it matches {string.Join(", ", otherKeywords.Select(binding => $"{NameOf(binding.Method)} ({string.Join("/", binding.Types)})").Distinct())}";
        }

        private static string NameOf(MethodInfo method) => $"{method.DeclaringType.Name}.{method.Name}";

        private static List<StepBinding> LoadBindings(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsDefined(typeof(BindingAttribute), inherit: true))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .SelectMany(method => method.GetCustomAttributes<StepDefinitionBaseAttribute>().Select(attribute => new StepBinding(method, RegexOf(attribute.Regex), attribute.Types)))
                .ToList();
        }

        // Reqnroll reads an expression without '^' or '$' and without the regex syntax it recognizes (such as '(.*)') as a
        // Cucumber expression, in which '(confirm|cancel)' is optional literal text, not a choice.
        private static Regex RegexOf(string expression)
        {
            return new CucumberExpressionDetector().IsCucumberExpression(expression)
                ? new ReqnrollCucumberExpression(expression, new CucumberExpressionParameterTypeRegistry(new BindingRegistry())).Regex
                : new Regex($"^{expression}$", RegexOptions.CultureInvariant);
        }

        private static IEnumerable<ScenarioStep> StepsOf(string featureFile)
        {
            Feature feature;
            using (var reader = new StreamReader(featureFile))
            {
                feature = new Parser().Parse(reader).Feature;
            }

            // A file without a feature (e.g. empty) has no steps.
            return feature == null ? Enumerable.Empty<ScenarioStep>() : StepsOf(feature.Children, new List<Step>(), featureFile);
        }

        // The steps of the scenario runs of a feature or a rule: a scenario runs once, a Scenario Outline once per Examples
        // row; each with the steps of its backgrounds first.
        private static IEnumerable<ScenarioStep> StepsOf(IEnumerable<IHasLocation> children, IReadOnlyList<Step> background, string featureFile)
        {
            var backgroundSteps = background.Concat(children.OfType<Background>().SelectMany(b => b.Steps)).ToList();
            foreach (var child in children)
            {
                if (child is Rule rule)
                {
                    foreach (var step in StepsOf(rule.Children, backgroundSteps, featureFile))
                    {
                        yield return step;
                    }
                }
                else if (child is Scenario scenario)
                {
                    var steps = backgroundSteps.Concat(scenario.Steps).ToList();
                    var rows = scenario.Examples.Any() ? scenario.Examples.SelectMany(RowsOf) : new[] { NoExamplesRow };
                    foreach (var step in rows.SelectMany(row => StepsOf(steps, row, featureFile)))
                    {
                        yield return step;
                    }
                }
            }
        }

        private static IEnumerable<IReadOnlyDictionary<string, string>> RowsOf(Examples examples)
        {
            var header = examples.TableHeader?.Cells.Select(cell => cell.Value).ToList() ?? new List<string>();
            return examples.TableBody.Select(row => header.Zip(row.Cells, (column, cell) => (column, cell.Value)).ToDictionary(pair => pair.column, pair => pair.Value));
        }

        // The steps with an Examples row's values filled in and their keywords resolved ('And' and 'But' take the previous
        // step's, as in Reqnroll).
        private static IEnumerable<ScenarioStep> StepsOf(IEnumerable<Step> steps, IReadOnlyDictionary<string, string> row, string featureFile)
        {
            var type = StepDefinitionType.Given;
            foreach (var step in steps)
            {
                type = step.KeywordType switch
                {
                    StepKeywordType.Context => StepDefinitionType.Given,
                    StepKeywordType.Action => StepDefinitionType.When,
                    StepKeywordType.Outcome => StepDefinitionType.Then,
                    _ => type,
                };
                var text = Placeholder.Replace(step.Text, match => row.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);
                yield return new ScenarioStep($"{featureFile}:{step.Location.Line}", type, text);
            }
        }

        private sealed record ScenarioStep(string Location, StepDefinitionType Type, string Text);

        private sealed record StepBinding(MethodInfo Method, Regex Regex, StepDefinitionType[] Types)
        {
            public bool Matches(ScenarioStep step) => this.Types.Contains(step.Type) && this.Regex.IsMatch(step.Text);
        }
    }
}
