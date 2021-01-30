using Roniz.WCF.P2P.Sync.Enums;

namespace Roniz.WCF.P2P.Sync
{
    /// <summary>
    /// contain default const values
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// The default value for "use generic resolver" property (true)
        /// </summary>
        public static bool UseGenericResolver = true;

        public const SynchronizationMode DefaultSynchronizationMode = SynchronizationMode.Reliable;
        /// <summary>
        /// The default value for re synchronization interval
        /// </summary>
        public const double ReSyncDefaultInterval = 30000;

        /// <summary>
        /// The endpoint name that should appear in the app.config of the application that consume this class
        /// </summary>
        public const string SyncDefaultEndpointConfigurationName = "SyncDefaultEndpointConfigurationName";
    }
}
