using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Xamarin.Essentials;

//[assembly: Xamarin.Forms.Dependency(typeof(Plugin.UWP.FileManagerImplementation))]
namespace Plugin.XFileManager
{
    /// <summary>
    /// Implementation for file picking on UWP
    /// </summary>
    class FileManagerImplementation : IXFileManager
    {
        private Dictionary<string, string> tokenDictionary;

        public FileManagerImplementation()
        {
            tokenDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Implementation for picking a folder on UWP platform.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all folder types
        /// can be selected while picking.
        /// On UWP, specify a list of extensions, like this: ".jpg", ".png".
        /// </param>
        /// <returns>
        /// Folder data object, or null when user cancelled picking folder
        /// </returns>
        public async Task<string> PickFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder == null)
            {
                return null;
            }
            var folderPath = folder.Path + "\\";
            // Application now has read/write access to all contents in the picked folder
            // (including other sub-folder contents)
            //        StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            var t = StorageApplicationPermissions.FutureAccessList.Add(folder);
            if (tokenDictionary.ContainsKey(folderPath))
            {
                tokenDictionary[folderPath] = t;
            }
            else
            {
                tokenDictionary.Add(folderPath, t);
            }

            return folderPath;

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

            UpdateFutureAccessList(file);

            return new FileData(file.Path, file.Path.Substring(0,
                file.Path.Length - file.Name.Length),
                file.Name,
                () => file.OpenStreamForReadAsync().Result);
        }
        
        public void UpdateFutureAccessList(StorageFile file)
        {
            var t = StorageApplicationPermissions.FutureAccessList.Add(file);
            if (tokenDictionary.ContainsKey(file.Path))
            {
                tokenDictionary[file.Path] = t;
            }
            else
            {
                tokenDictionary.Add(file.Path, t);
            }
        }

        /// <summary>
        /// UWP implementation of saving a picked file to the app's local folder directory.
        /// </summary>
        /// <param name="fileToSave">picked file data for file to save</param>
        /// <returns>true when file was saved successfully, false when not</returns>
        public async Task<bool> SaveFileToLocalAppStorage(FileData fileToSave)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    fileToSave.FileName,
                    CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(file, fileToSave.DataArray);

                UpdateFutureAccessList(file);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// UWP implementation of saving a file to previously picked folder directory or subdirectories. Please note, PickFolder must be called on the root folder
        /// </summary>
        /// <param name="fileToSave">picked file data for file to save</param>
        /// <returns>true when file was saved successfully, false when not</returns>
        public async Task<bool> SaveFileInFolder(FileData fileToSave)
        {
            var inFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tokenDictionary[fileToSave.FolderPath]);


            var outFile = await inFolder.CreateFileAsync(fileToSave.FileName, CreationCollisionOption.ReplaceExisting);
            var outFileStream = await outFile.OpenStreamForWriteAsync();

            var tempsteam = fileToSave.GetStream();
            await tempsteam.CopyToAsync(outFileStream);
            tempsteam.Flush();
            tempsteam.Close();
            outFileStream.Flush();
            outFileStream.Close();

            UpdateFutureAccessList(outFile);


            //await File.WriteAllBytesAsync(fileToSave.FilePath, fileToSave.DataArray);
            return true;
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
                    await Windows.System.Launcher.LaunchFileAsync(file);
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
                    await Windows.System.Launcher.LaunchFileAsync(file);
                }
            }
            catch (FileNotFoundException)
            {
                await this.SaveFileToLocalAppStorage(fileToOpen);
                this.OpenFile(fileToOpen);
            }
            catch (Exception)
            {
                // ignore exceptions
            }
        }



        /// <summary>
        /// Implementation for getting FileData of a file on uwp.
        /// </summary>
        /// <param name="filePath">
        /// Specifies the file from which the stream should be opened.
        /// <returns>stream object</returns>
        public async Task<(bool,FileData)> GetFileDataFromPath(string filePath)
        {
            string token = tokenDictionary[filePath];

            if (token != "")
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);

                return (true, new FileData(file.Path, Path.GetDirectoryName(file.Path), file.Name, () => file.OpenStreamForReadAsync().Result));
                //return file.OpenStreamForReadAsync().Result;
            }
            return (false, null);
        }

        public async void OpenFileViaEssentials(string fileToOpen)
        {
            await Xamarin.Essentials.Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(fileToOpen)
            });
        }

        public string GetLocalAppFolder()
        {
            return ApplicationData.Current.LocalFolder.Path + "\\";
        }
    }
}
