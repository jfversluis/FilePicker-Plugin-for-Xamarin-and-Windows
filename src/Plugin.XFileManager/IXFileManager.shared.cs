using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.XFileManager
{
    public static class FileManager
    {
        public async static Task<(bool, string)> PickFolder()
        {
            try
            {
                   var currPickedFile = await XFileManager.Current.PickFolder().ConfigureAwait(true);

                if (currPickedFile == null)
                    return (false, null); // user canceled file picking
                return (true, currPickedFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return (false, null);
            }
        }
        public async static Task<(bool, FileData)> PickFile(string[] allowedTypes)
        {
            try
            {
                var currPickedFile = await XFileManager.Current.PickFile(allowedTypes).ConfigureAwait(true);

                if (currPickedFile == null)
                    return (false, null); // user canceled file picking
                return (true, currPickedFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return (false, null);
            }
        }
        public async static Task<(bool, FileData)> GetFileDataFromPath(string filePath)
        {
            try
            {
                (bool success, FileData file) = await XFileManager.Current.GetFileDataFromPath(filePath).ConfigureAwait(true);
                if (success)
                {
                    return (true, file);
                }
                else
                {
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return (false, null);
            }
        }
        public async static Task<(bool, MemoryStream)> GetStreamFromPath(string filePath)
        {
            try
            {
                (bool success, FileData file) = await XFileManager.Current.GetFileDataFromPath(filePath).ConfigureAwait(true);
                if (success)
                {
                    return (true, new MemoryStream(file.DataArray));
                }
                else
                {
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return (false, null);
            }
        }
        public async static Task<bool> SaveFileToLocalAppStorage(FileData newFile)
        {
            try
            {
                var currPickedFile = await XFileManager.Current.SaveFileToLocalAppStorage(newFile).ConfigureAwait(true);
                return currPickedFile;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        public async static Task<bool> SaveFileInFolder(FileData newFile)
        {
            try
            {
                var currPickedFile = await XFileManager.Current.SaveFileInFolder(newFile).ConfigureAwait(true);

                return currPickedFile;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        public async static Task<bool> OpenFile(string filePath)
        {
            try
            {
                await XFileManager.Current.OpenFileViaEssentials(filePath);

                //if (currPickedFile == null)
                //    return (false, null); // user canceled file picking
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        public static string GetLocalAppFolder()
        {
            try
            {
                return XFileManager.Current.GetLocalAppFolder();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "";
            }

        }

    }
}

namespace Plugin.XFileManager
{
    /// <summary>
    /// Interface for XFileManager plugin. Access the platform specific instance
    /// of this interface by using CrossXFileManager.Current.
    /// </summary>
    public interface IXFileManager
    {
        /// <summary>
        /// Starts folder picking and returns folder data for picked folder. Folder
        /// types can be specified in order to limit folders that can be
        /// selected. Note that this method may throw exceptions that occured
        /// during folder picking.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all folder types
        /// can be selected while picking.
        /// On Android you can specify one or more MIME types, e.g.
        /// "image/png"; also wild card characters can be used, e.g. "image/*".
        /// On iOS you can specify UTType constants, e.g. UTType.Image.
        /// On UWP, specify a list of extensions, like this: ".jpg", ".png".
        /// On WPF, specify strings like this: "Data type (*.ext)|*.ext", which
        /// corresponds how the Windows folder open dialog specifies folder types.
        /// </param>
        /// <returns>
        /// Folder data object, or null when user cancelled picking folder
        /// </returns>
        Task<string> PickFolder();
        //windows verified


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
        //windows verified

        /// <summary>
        /// Gets stream to access the file from the path.
        /// Note that when DataArray property was already accessed, the stream
        /// must be rewinded to the beginning.
        /// </summary>
        /// <param name="filePath">
        /// Specifies the file from which the stream should be opened.
        /// <returns>stream object</returns>
        Task<(bool, FileData)> GetFileDataFromPath(string filePath);
        //windows verified

        /// <summary>
        /// Saves the file that was picked to external storage.
        /// </summary>
        /// <param name="fileToSave">
        /// File data from a call to PickFile() that should be saved.
        /// </param>
        /// <returns>
        /// True when file was saved successfully, false when there was an
        /// error
        /// </returns>
        //[Obsolete("The SaveFile() method is obsolete; store the picked file with system functions from System.IO or read from stream directly")]
        Task<bool> SaveFileToLocalAppStorage(FileData fileToSave);
        //windows verified

        Task<bool> SaveFileInFolder(FileData fileToSave);

        Task<bool> OpenFileViaEssentials(string fileToOpen);
        //windows verified

        string GetLocalAppFolder();

    }
}


