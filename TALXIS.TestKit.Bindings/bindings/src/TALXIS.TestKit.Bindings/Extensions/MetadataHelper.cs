using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using Reqnroll;

namespace TALXIS.TestKit.Bindings.Extensions
{
    internal class MetadataHelper : PowerAppsStepDefiner
    {
        /// <summary>
        /// Gets the field type from the DOM using a JavaScript query.
        /// </summary>
        internal static string GetFieldTypeFromDomByLable(string fieldLabel)
        {
            string fieldLogicalName = XrmApp.Entity.GetFieldLogicalNameFromLabel(fieldLabel);

            return GetFieldTypeFromDomByLogicalName(fieldLogicalName);
        }

        internal static string GetFieldTypeFromDomByLogicalName(string fieldLogicalName)
        {
            return XrmApp.Entity.GetFieldType(fieldLogicalName);
        }

        internal static string GetFieldLocationFromDomByLogicalName(string fieldLogicalName)
        {
            return XrmApp.Entity.GetFieldLocation(fieldLogicalName);
        }
    }
}
