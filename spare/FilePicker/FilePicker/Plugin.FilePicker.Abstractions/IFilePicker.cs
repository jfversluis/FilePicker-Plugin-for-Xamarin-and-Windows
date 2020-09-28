using System.Threading.Tasks;

namespace Plugin.FilePicker.Abstractions
{
    /// <summary>
    /// Interface for FilePicker
    /// </summary>
    public interface IFilePicker
    {
        /// <summary>
        /// Starts file picking and returns file data for picked file. File
        /// types can be specified in order to limit files that can be
        /// selected. Note that this method may throw exceptions that occured
        /// during file picking.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On Android you can specify one or more MIME types, e.g.
        /// "image/png"; also wild card characters can be used, e.g. "image/*".
        /// On iOS you can specify UTType constants, e.g. UTType.Image.
        /// On UWP, specify a list of extensions, like this: ".jpg", ".png".
        /// On WPF, specify strings like this: "Data type (*.ext)|*.ext", which
        /// corresponds how the Windows file open dialog specifies file types.
        /// </param>
        /// <returns>
        /// File data object, or null when user cancelled picking file
        /// </returns>
        Task<FileData> PickFile(string[] allowedTypes = null);

        /// <summary>
        /// Starts file picking and returns file data for created or existing file. File
        /// types can be specified in order to limit files that can be
        /// created. Note that this method may throw exceptions that occured
        /// during file picking.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On Android you can specify one or more MIME types, e.g.
        /// "image/png"; also wild card characters can be used, e.g. "image/*".
        /// On iOS you can specify UTType constants, e.g. UTType.Image.
        /// On UWP, specify a list of extensions, like this: ".jpg", ".png".
        /// On WPF, specify strings like this: "Data type (*.ext)|*.ext", which
        /// corresponds how the Windows file open dialog specifies file types.
        /// </param>
        /// <returns>
        /// File data object, or null when user cancelled picking file
        /// </returns>
        Task<FilePlaceholder> CreateOrOverwriteFile(string[] allowedTypes = null);
    }
}