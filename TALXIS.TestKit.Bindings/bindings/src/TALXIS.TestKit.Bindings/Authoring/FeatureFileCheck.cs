namespace TALXIS.TestKit.Bindings.Authoring
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using CucumberExpressions;
    using Gherkin;
    using Gherkin.Ast;
    using Reqnroll;
    using Reqnroll.Bindings;
    using Reqnroll.Bindings.CucumberExpressions;
    using Reqnroll.Bindings.Reflection;
    using Reqnroll.Configuration;

    /// <summary>
    /// Checks feature files against the step bindings without a browser or an environment. A step that matches no binding
    /// or several otherwise only fails when its scenario runs, possibly late in a long test run.
    /// </summary>
    public static class FeatureFileCheck
    {
        private const BindingFlags StepMethods = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static readonly Regex Placeholder = new Regex("<([^<>]+)>");
        private static readonly IReadOnlyDictionary<string, string> NoExamplesRow = new Dictionary<string, string>();

        /// <summary>
        /// Finds the steps that match no step binding or more than one. The steps are matched as Reqnroll matches them: by
        /// their keyword ('And' and 'But' take the previous step's), with the values of each Examples row filled in,
        /// against the binding's regular expression, Cucumber expression (with the parameter types of step argument
        /// transformations and enums) or method name, and to a binding whose method takes the step's arguments. Scoped
        /// bindings ([Scope]) are matched as unscoped ones.
        /// </summary>
        /// <param name="featureFiles">The paths of the feature files.</param>
        /// <param name="bindingAssemblies">The assemblies with step bindings: the test project's and those that reqnroll.json
        /// lists (such as TALXIS.TestKit.Bindings).</param>
        /// <param name="featureLanguage">The language of feature files without a '# language:' line (reqnroll.json's
        /// language.feature).</param>
        /// <returns>One line per binding that Reqnroll cannot use and per such step: where it is, its text and the
        /// bindings it matches.</returns>
        public static IReadOnlyList<string> FindStepsWithoutOneBinding(IEnumerable<string> featureFiles, IEnumerable<Assembly> bindingAssemblies, string featureLanguage = "en")
        {
            if (featureFiles == null)
            {
                throw new ArgumentNullException(nameof(featureFiles));
            }

            if (bindingAssemblies == null)
            {
                throw new ArgumentNullException(nameof(bindingAssemblies));
            }

            // A wrong folder would otherwise pass the check without checking anything.
            var files = featureFiles.ToList();
            if (files.Count == 0)
            {
                throw new ArgumentException("There are no feature files to check.", nameof(featureFiles));
            }

            var problems = new List<string>();
            var bindings = LoadBindings(bindingAssemblies.ToList(), problems);
            problems.AddRange(files
                .SelectMany(file => StepsOf(file, featureLanguage))
                .Select(step => ProblemOf(step, bindings))
                .Where(problem => problem != null));

            return problems.Distinct().ToList();
        }

        // As in Reqnroll: the bindings of the step's keyword whose expression matches its text, of which the one whose
        // method takes the step's arguments (the expression's values, and its data table or doc string) runs.
        private static string ProblemOf(ScenarioStep step, IReadOnlyList<StepBinding> bindings)
        {
            var matching = bindings.Where(binding => binding.Type == step.Type && binding.Regex.IsMatch(step.Text)).ToList();
            var fitting = matching.Where(binding => binding.Takes(step)).Select(binding => binding.Method).Distinct().ToList();

            if (fitting.Count == 1)
            {
                return null;
            }

            if (fitting.Count > 1)
            {
                return $"{step.Location}: '{step.Text}' matches {string.Join(", ", fitting.Select(NameOf))}";
            }

            if (matching.Count > 0)
            {
                var takes = matching.Select(binding => binding.Method).Distinct().Select(method => $"{NameOf(method)} takes {method.GetParameters().Length}");
                return $"{step.Location}: '{step.Text}' gives {matching[0].ArgumentsOf(step)} argument(s), but {string.Join(", ", takes)}";
            }

            return NoBindingFor(step, bindings);
        }

        // Reqnroll matches a step only to a binding of its keyword: 'Given ... And I select the tab' does not run a
        // [When] binding of 'I select the tab'.
        private static string NoBindingFor(ScenarioStep step, IReadOnlyList<StepBinding> bindings)
        {
            var otherKeywords = bindings.Where(binding => binding.Regex.IsMatch(step.Text)).ToList();
            return otherKeywords.Count == 0
                ? $"{step.Location}: no binding for '{step.Text}'"
                : $"{step.Location}: no {step.Type} binding for '{step.Text}'; it matches {string.Join(", ", otherKeywords.Select(binding => $"{NameOf(binding.Method)} ({binding.Type})").Distinct())}";
        }

        // A binding class has the step methods it declares and those it inherits.
        private static string NameOf(MethodInfo method) => $"{method.ReflectedType.Name}.{method.Name}";

        // The step bindings as Reqnroll builds them from the binding classes.
        private static List<StepBinding> LoadBindings(IReadOnlyList<Assembly> assemblies, List<string> problems)
        {
            var methods = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsDefined(typeof(BindingAttribute), inherit: true))
                .SelectMany(type => type.GetMethods(StepMethods))
                .ToList();
            var stepMethods = methods.Where(method => method.IsDefined(typeof(StepDefinitionBaseAttribute), inherit: true)).ToList();
            var parameterTypes = ParameterTypesOf(methods, stepMethods);
            var methodNames = new StepDefinitionRegexCalculator(ConfigurationLoader.GetDefault());

            var bindings = new List<StepBinding>();
            foreach (var method in stepMethods)
            {
                foreach (var attribute in method.GetCustomAttributes<StepDefinitionBaseAttribute>())
                {
                    foreach (var type in attribute.Types)
                    {
                        try
                        {
                            bindings.Add(new StepBinding(method, type, RegexOf(attribute.Regex, type, method, parameterTypes, methodNames)));
                        }
                        catch (Exception e) when (e is CucumberExpressionException || e is ArgumentException)
                        {
                            // Reqnroll fails the steps of a binding whose expression it cannot read.
                            problems.Add($"{NameOf(method)}: {e.Message.Split('\n')[0].Trim()}");
                        }
                    }
                }
            }

            return bindings;
        }

        // The parameter types of Cucumber expressions: the built-in ones, those of the step argument transformations, and
        // the enums that step methods take, which have to be known before the first expression is read.
        private static CucumberExpressionParameterTypeRegistry ParameterTypesOf(IEnumerable<MethodInfo> methods, IEnumerable<MethodInfo> stepMethods)
        {
            var registry = new BindingRegistry();
            foreach (var method in methods)
            {
                foreach (var attribute in method.GetCustomAttributes<StepArgumentTransformationAttribute>())
                {
                    registry.RegisterStepArgumentTransformationBinding(new StepArgumentTransformationBinding(attribute.Regex, new RuntimeBindingMethod(method), attribute.Name));
                }
            }

            var parameterTypes = new CucumberExpressionParameterTypeRegistry(registry);
            foreach (var method in stepMethods)
            {
                parameterTypes.OnBindingMethodProcessed(new RuntimeBindingMethod(method));
            }

            return parameterTypes;
        }

        // Reqnroll reads an expression without '^' or '$' and without the regex syntax it recognizes (such as '(.*)') as a
        // Cucumber expression, in which '(confirm|cancel)' is optional literal text, not a choice. A binding without an
        // expression matches its method's name.
        private static Regex RegexOf(string expression, StepDefinitionType type, MethodInfo method, CucumberExpressionParameterTypeRegistry parameterTypes, StepDefinitionRegexCalculator methodNames)
        {
            if (expression == null)
            {
                return new Regex(methodNames.CalculateRegexFromMethod(type, new RuntimeBindingMethod(method)), RegexOptions.CultureInvariant);
            }

            return new CucumberExpressionDetector().IsCucumberExpression(expression)
                ? new ReqnrollCucumberExpression(expression, parameterTypes).Regex
                : new Regex($"^{expression}$", RegexOptions.CultureInvariant);
        }

        private static IEnumerable<ScenarioStep> StepsOf(string featureFile, string featureLanguage)
        {
            Feature feature;
            try
            {
                using (var reader = new StreamReader(featureFile))
                {
                    feature = new Parser().Parse(new TokenScanner(reader), new TokenMatcher(featureLanguage)).Feature;
                }
            }
            catch (ParserException e)
            {
                throw new FormatException($"{featureFile}: {e.Message}", e);
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
                yield return new ScenarioStep($"{featureFile}:{step.Location.Line}", type, text, step.Argument != null);
            }
        }

        private sealed record ScenarioStep(string Location, StepDefinitionType Type, string Text, bool HasArgument);

        private sealed record StepBinding(MethodInfo Method, StepDefinitionType Type, Regex Regex)
        {
            // The values the expression matches in the step's text, and the step's data table or doc string.
            public int ArgumentsOf(ScenarioStep step) => this.Regex.Match(step.Text).Groups.Count - 1 + (step.HasArgument ? 1 : 0);

            public bool Takes(ScenarioStep step) => this.ArgumentsOf(step) == this.Method.GetParameters().Length;
        }
    }
}
