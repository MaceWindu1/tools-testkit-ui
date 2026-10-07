namespace TALXIS.TestKit.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Microsoft.Xrm.Sdk;
    using OpenQA.Selenium;

    /// <summary>
    /// Assists in test setup.
    /// </summary>
    public class TestDriver : ITestDriver
    {
        private const string DriverScriptPath = "driver.js";
        private const string TestDriverReference = "top.driver";
        private const string ErrorPrefix = "driver encountered an error";
        private const string LibraryNamespace = "TALXISTestKitBindings";
        private const string DataManagerStateScript = "return JSON.stringify({ refs: top.dataManager.refs, refsByAlias: top.dataManager.refsByAlias });";

        private readonly IJavaScriptExecutor javascriptExecutor;

        private string path;
        private string authToken;
        private string dataverseUrl;
        private bool suppressFlows;

        // References to the created test records, kept outside the page so that they survive navigation.
        private string dataManagerState;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDriver"/> class.
        /// Injects the specflow.driver.js library using the provided <see cref="IJavaScriptExecutor"/>.
        /// </summary>
        /// <param name="javascriptExecutor">The IJavaScriptExecutor to use.</param>
        public TestDriver(IJavaScriptExecutor javascriptExecutor)
        {
            this.javascriptExecutor = javascriptExecutor;
        }

        private string FilePath => this.path ?? (this.path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DriverScriptPath));

        /// <inheritdoc/>
        public void InjectOnPage(string authToken, string dataverseUrl, bool suppressFlows)
        {
            this.authToken = authToken;
            this.dataverseUrl = dataverseUrl;
            this.suppressFlows = suppressFlows;
            var suppressFlowsArgument = suppressFlows ? "true" : "false";

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(File.ReadAllText(this.FilePath));

            if (!string.IsNullOrEmpty(dataverseUrl))
            {
                if (string.IsNullOrEmpty(authToken))
                {
                    throw new ArgumentException("An application user auth token is required to create test data in a separate Dataverse environment.", nameof(authToken));
                }

                // The test data lives in a Dataverse environment that is not the page's origin (e.g. behind a TALXIS Portal),
                // so all data operations go through that environment's Web API as the application user.
                var webApiUrl = $"{dataverseUrl.TrimEnd('/')}/api/data/v9.1/";
                scriptBuilder.AppendLine(
                    $@"top.metadataRepository = new {LibraryNamespace}.MetadataRepository('{webApiUrl}', '{authToken}');
                       top.recordRepository = new {LibraryNamespace}.AuthenticatedRecordRepository(top.metadataRepository, '{authToken}', undefined, '{webApiUrl}', {suppressFlowsArgument});
                       top.deepInsertService = new {LibraryNamespace}.DeepInsertService(top.metadataRepository, top.recordRepository);
                       top.dataManager = new {LibraryNamespace}.DataManager(top.recordRepository, top.deepInsertService, [new {LibraryNamespace}.FakerPreprocessor()], top.recordRepository);");
            }
            else
            {
                scriptBuilder.AppendLine($@"top.recordRepository = new {LibraryNamespace}.CurrentUserRecordRepository(Xrm.WebApi.online);
                       top.metadataRepository = new {LibraryNamespace}.MetadataRepository();
                       top.deepInsertService = new {LibraryNamespace}.DeepInsertService(top.metadataRepository, top.recordRepository);");
                if (!string.IsNullOrEmpty(authToken))
                {
                    scriptBuilder.AppendLine(
                        $@"top.appUserRecordRepository = new {LibraryNamespace}.AuthenticatedRecordRepository(top.metadataRepository, '{authToken}', undefined, undefined, {suppressFlowsArgument});
                           top.dataManager = new {LibraryNamespace}.DataManager(top.recordRepository, top.deepInsertService, [new {LibraryNamespace}.FakerPreprocessor()], top.appUserRecordRepository);");
                }
                else
                {
                    scriptBuilder.AppendLine(
                        $"top.dataManager = new {LibraryNamespace}.DataManager(top.recordRepository, top.deepInsertService, [new {LibraryNamespace}.FakerPreprocessor()]);");
                }
            }

            scriptBuilder.AppendLine($"{TestDriverReference} = new {LibraryNamespace}.Driver(top.dataManager);");

            if (this.dataManagerState != null)
            {
                scriptBuilder.AppendLine(
                    $"(state => {{ top.dataManager.refs.push(...state.refs); Object.assign(top.dataManager.refsByAlias, state.refsByAlias); }})({this.dataManagerState});");
            }

            this.javascriptExecutor.ExecuteScript(scriptBuilder.ToString());
        }

        /// <inheritdoc cref="ITestDriver"/>
        public void LoadTestData(string data)
        {
            this.ExecuteDriverFunctionAsync($"loadTestData(`{data}`)");
            this.SaveDataManagerState();
        }

        /// <inheritdoc/>
        public void LoadTestDataAsUser(string data, string username)
        {
            this.ExecuteDriverFunctionAsync($"loadTestDataAsUser(`{data}`, '{username}')");
            this.SaveDataManagerState();
        }

        /// <inheritdoc cref="ITestDriver"/>
        public void DeleteTestData()
        {
            this.ExecuteDriverFunctionAsync("deleteTestData()");
            this.dataManagerState = null;
        }

        /// <inheritdoc cref="ITestDriver"/>
        public void OpenTestRecord(string recordAlias)
        {
            this.ExecuteDriverFunctionAsync($"openTestRecord('{recordAlias}')");
        }

        /// <inheritdoc/>
        public EntityReference GetTestRecordReference(string recordAlias)
        {
            var obj = (Dictionary<string, object>)this.ExecuteDriverFunction($"getRecordReference('{recordAlias}')");

            return new EntityReference((string)obj["entityType"], Guid.Parse((string)obj["id"]));
        }

        private static string GetExecuteScriptForAsyncFunction(string functionCall)
        {
            return $"{TestDriverReference}.{functionCall}.then(arguments[arguments.length - 1]).catch(e => {{ arguments[arguments.length - 1](`{ErrorPrefix}: ${{ e.message }}`); }});";
        }

        private object ExecuteDriverFunction(string functionCall)
        {
            this.EnsureInjected();

            return this.javascriptExecutor.ExecuteScript($"return {TestDriverReference}.{functionCall};");
        }

        private object ExecuteDriverFunctionAsync(string functionCall)
        {
            this.EnsureInjected();

            var result = this.javascriptExecutor.ExecuteAsyncScript(GetExecuteScriptForAsyncFunction(functionCall));

            if (result is string str && str.StartsWith(ErrorPrefix, StringComparison.InvariantCulture))
            {
                throw new WebDriverException(str);
            }

            return result;
        }

        /// <summary>
        /// Re-injects the driver (with the references of the records created so far) if a page navigation removed it.
        /// </summary>
        private void EnsureInjected()
        {
            var isInjected = (bool)this.javascriptExecutor.ExecuteScript($"return typeof {TestDriverReference} !== 'undefined';");
            if (!isInjected)
            {
                this.InjectOnPage(this.authToken, this.dataverseUrl, this.suppressFlows);
            }
        }

        private void SaveDataManagerState()
        {
            this.dataManagerState = (string)this.javascriptExecutor.ExecuteScript(DataManagerStateScript);
        }
    }
}