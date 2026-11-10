using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using TALXIS.TestKit.Selectors.Browser;

namespace TALXIS.TestKit.Selectors.DTO.Locators
{
    internal class CommandBarElementsLocators
    {
        internal static By Container => By.XPath(AppElements.Xpath[AppReference.CommandBar.Container]);
        internal static By ContainerGrid => By.XPath(AppElements.Xpath[AppReference.CommandBar.ContainerGrid]);
        internal static By MoreCommandsMenu => By.XPath(AppElements.Xpath[AppReference.CommandBar.MoreCommandsMenu]);
        internal static By Button(string name) => By.XPath(AppElements.Xpath[AppReference.CommandBar.Button].Replace("[NAME]", name));
        internal static By OverflowButton => By.XPath(AppElements.Xpath[AppReference.CommandBar.OverflowButton]);
        internal static By CommandItem => By.XPath(AppElements.Xpath[AppReference.CommandBar.CommandItem]);
        internal static By Flyout(string commandItemId) => By.XPath(AppElements.Xpath[AppReference.CommandBar.Flyout].Replace("[ID]", (commandItemId + "_flyout").ToXPathLiteral()));
        internal static By FlyoutOpener(string name) => By.XPath(AppElements.Xpath[AppReference.CommandBar.FlyoutOpener].Replace("[NAME]", name.ToXPathLiteral()));
        internal static By FlyoutCommand(string name) => By.XPath(AppElements.Xpath[AppReference.CommandBar.FlyoutCommand].Replace("[NAME]", name.ToXPathLiteral()));
    }
}
