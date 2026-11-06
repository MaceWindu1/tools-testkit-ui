namespace TALXIS.TestKit.Bindings.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using OpenQA.Selenium;

    /// <summary>
    /// Reads the browser's console, where the errors of page scripts (e.g. a form script that failed) end up.
    /// </summary>
    public static class BrowserConsoleExtensions
    {
        /// <summary>
        /// Reads the console messages logged since the console was last read. The browser keeps them only when it was
        /// started with browser logging, as <see cref="Configuration.BrowserOptionsWithProfileSupport"/> starts Chrome.
        /// </summary>
        /// <param name="driver">The web driver.</param>
        /// <returns>The messages, each with its time and level.</returns>
        public static IReadOnlyList<string> ReadBrowserConsole(this IWebDriver driver)
        {
            return driver.Manage().Logs.GetLog(LogType.Browser)
                .Select(entry => $"{entry.Timestamp:HH:mm:ss.fff} {entry.Level} {entry.Message}")
                .ToList();
        }
    }
}
