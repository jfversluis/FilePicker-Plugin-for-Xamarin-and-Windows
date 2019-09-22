using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for file picking on UWP
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        private Dictionary<string, string> tokenDictionary;

        public FilePickerImplementation()
        {
            tokenDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Implementation for picking a file on UWP platform.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On UWP, specify a list of extensions, like this: ".jpg", ".png".
        /// </param>
        /// <returns>
        /// File data object, or null when user cancelled picking file
        /// </returns>
        public async Task<FileData> PickFile(string[] allowedTypes)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            if (allowedTypes != null)
            {
                var hasAtleastOneType = false;

                foreach (var type in allowedTypes)
                {
                    if (type.StartsWith("."))
                    {
                        picker.FileTypeFilter.Add(type);
                        hasAtleastOneType = true;
                    }
                }

                if (!hasAtleastOneType)
                {
                    picker.FileTypeFilter.Add("*");
                }
            }
            else
            {
                picker.FileTypeFilter.Add("*");
            }

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return null;
            }
            
            var t = StorageApplicationPermissions.FutureAccessList.Add(file);
            tokenDictionary.Add(file.Path, t);
            return new FileData(file.Path, file.Name, () => GetStream(file.Path).Result);
        }

        /// <summary>
        /// UWP implementation of saving a picked file to the app's local folder directory.
        /// </summary>
        /// <param name="fileToSave">picked file data for file to save</param>
        /// <returns>true when file was saved successfully, false when not</returns>
        public async Task<bool> SaveFile(FileData fileToSave)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    fileToSave.FileName,
                    CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(file, fileToSave.DataArray);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// UWP implementation of OpenFile(), opening a file already stored in the app's local
        /// folder directory.
        /// storage.
        /// </summary>
        /// <param name="fileToOpen">relative filename of file to open</param>
        public async void OpenFile(string fileToOpen)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileToOpen);

                if (file != null)
                {
                    await Launcher.LaunchFileAsync(file);
                }
            }
            catch (FileNotFoundException)
            {
                // ignore exceptions
            }
            catch (Exception)
            {
                // ignore exceptions
            }
        }

        /// <summary>
        /// UWP implementation of OpenFile(), opening a picked file in an external viewer. The
        /// picked file is saved  the app's local folder directory.
        /// </summary>
        /// <param name="fileToOpen">picked file data</param>
        public async void OpenFile(FileData fileToOpen)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileToOpen.FileName);

                if (file != null)
                {
                    await Launcher.LaunchFileAsync(file);
                }
            }
            catch (FileNotFoundException)
            {
                await this.SaveFile(fileToOpen);
                this.OpenFile(fileToOpen);
            }
            catch (Exception)
            {
                // ignore exceptions
            }
        }

        /// <summary>
        /// Implementation for getting a stream of a file on uwp.
        /// </summary>
        /// <param name="filePath">
        /// Specifies the file from which the stream should be opened.
        /// <returns>stream object</returns>
        public async Task<Stream> GetStream(string filePath)
        {
            string token = tokenDictionary[filePath];

            if (token != "")
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);

                return file.OpenStreamForReadAsync().Result;
            }
            return Stream.Null;
        }
    }
}
