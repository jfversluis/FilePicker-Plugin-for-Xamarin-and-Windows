using System;

namespace Plugin.XFileManager
{
    /// <summary>
    /// Cross-platform XFileManager implementation
    /// </summary>
    public static class XFileManager
    {
        /// <summary>
        /// Lazy-initialized file picker implementation
        /// </summary>
        private static Lazy<IXFileManager> implementation =
            new Lazy<IXFileManager>(CreateXFileManager, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current file picker plugin implementation to use
        /// </summary>
        public static IXFileManager Current
        {
            get
            {
                var ret = implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }

                return ret;
            }
            //set
            //{
            //    implementation = new Lazy<IXFileManager>(() => value);
            //}
        }

        /// <summary>
        /// Creates file picker instance for the platform
        /// </summary>
        /// <returns>file picker instance</returns>
        private static IXFileManager CreateXFileManager()
        {

        #if NETSTANDARD1_0 || NETSTANDARD2_0// || PORTABLE
            return null;
            #else
                return new FileManagerImplementation();
            #endif
        }

        /// <summary>
        /// Returns new exception to throw when implementation is not found. This is the case when
        /// the NuGet package is not added to the platform specific project.
        /// </summary>
        /// <returns>exception to throw</returns>
        internal static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException(
                "This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
}
