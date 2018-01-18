using Plugin.FilePicker.Abstractions;
using System;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Cross-platform FilePicker implementations
    /// </summary>
    public class CrossFilePicker
    {
        private static Lazy<IFilePicker> Implementation = new Lazy<IFilePicker>(CreateFilePicker, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static IFilePicker Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        private static IFilePicker CreateFilePicker()
        {
#if PORTABLE
            return null;
#else
            return new FilePickerImplementation();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}