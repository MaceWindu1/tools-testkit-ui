namespace TALXIS.TestKit.Bindings.Configuration
{
    /// <summary>
    /// The kind of app the tests run against. Steps whose text does not name it act on the configured target, so that the
    /// same feature file can run against a model-driven app and a TALXIS Portal showing the same forms.
    /// </summary>
    public enum TestTarget
    {
        /// <summary>
        /// A model-driven app.
        /// </summary>
        ModelDrivenApp,

        /// <summary>
        /// A TALXIS Portal.
        /// </summary>
        Portal,
    }
}
