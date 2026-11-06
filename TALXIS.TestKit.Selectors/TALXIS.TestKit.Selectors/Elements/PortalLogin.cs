using System;
using System.Security;
using TALXIS.TestKit.Selectors.Browser;
using TALXIS.TestKit.Selectors.WebClientManagement;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// Signs in to a TALXIS Portal through its Azure AD B2C local-account sign-in page.
    /// </summary>
    public class PortalLogin : Element, ILogin
    {
        private readonly PortalLoginManager _loginManager;

        public PortalLogin(WebClient client) : base()
        {
            _loginManager = new PortalLoginManager(client);
        }

        /// <inheritdoc/>
        /// <remarks>B2C local accounts ask for no one-time code, so <paramref name="otpToken"/> is not used.</remarks>
        public void LogIn(Uri url, SecureString username, SecureString password, SecureString otpToken = null)
        {
            _loginManager.LogIn(url, username, password);
        }
    }
}
