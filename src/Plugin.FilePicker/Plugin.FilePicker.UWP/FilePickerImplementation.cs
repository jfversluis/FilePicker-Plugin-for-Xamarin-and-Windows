using Plugin.FilePicker.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        public async Task<FileData> PickFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            var fileTypeFilters = GetFileTypeFilters();
            if (fileTypeFilters == null)
            {
                picker.FileTypeFilter.Add("*");
            }
            else
            {
                foreach(string filter in fileTypeFilters)
                {
                    picker.FileTypeFilter.Add(filter);
                }
            }
           

            var file = await picker.PickSingleFileAsync();
            if (null == file)
                return null;

            StorageApplicationPermissions.FutureAccessList.Add(file);
            return new FileData(file.Path, file.Name, () => File.OpenRead(file.Path));
        }
        
        public async Task<bool> SaveFile(FileData fileToSave)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileToSave.FileName, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(file, fileToSave.DataArray);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

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
            catch (FileNotFoundException ex)
            {
            }
            catch (Exception ex)
            {
            }
        }

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
            catch (FileNotFoundException ex)
            {
                await SaveFile(fileToOpen);
                OpenFile(fileToOpen);
            }
            catch (Exception ex)
            {
            }
        }

        #region Protected Virtual Methods

        /// <summary>
        /// Override this method to specify file type filters for UWP
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetFileTypeFilters()
        {
            return null;
        }

        #endregion


    }
}