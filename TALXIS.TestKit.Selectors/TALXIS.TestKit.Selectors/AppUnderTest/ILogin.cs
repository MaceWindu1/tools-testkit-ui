using System;
using System.Security;

namespace TALXIS.TestKit.Selectors
{
    /// <summary>
    /// Signs in to the app under test.
    /// </summary>
    public interface ILogin
    {
        /// <summary>
        /// Opens the app at a URL, signs in and waits until the app is ready to be used.
        /// </summary>
        /// <param name="url">The URL of the app.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="otpToken">The secret key of the one-time codes, when the sign-in asks for one.</param>
        void LogIn(Uri url, SecureString username, SecureString password, SecureString otpToken = null);
    }
}
