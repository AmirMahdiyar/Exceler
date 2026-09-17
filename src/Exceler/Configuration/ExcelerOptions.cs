namespace Exceler.Configuration
{
    /// <summary>
    /// Internal options representing framework-wide runtime configuration.
    /// </summary>
    internal class ExcelerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the EPPlus license context has been explicitly configured in DI.
        /// </summary>
        public bool IsLicenseConfigured { get; set; }
    }
}
