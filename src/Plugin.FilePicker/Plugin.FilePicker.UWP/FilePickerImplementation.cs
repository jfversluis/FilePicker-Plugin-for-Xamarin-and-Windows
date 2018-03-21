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
            return await TakeMediaAsync(0);
        }

        public async Task<FileData> PickFile(int maximumFileSize)
        {
            return await TakeMediaAsync(maximumFileSize);
        }

        private async Task<FileData> TakeMediaAsync(int maximumFileSize)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");

            var file = await picker.PickSingleFileAsync();
            if (null == file)
                return null;

            StorageApplicationPermissions.FutureAccessList.Add(file);

            var fileProperties = await file.GetBasicPropertiesAsync();
            var isFileSizeTooLarge = false;
            if (maximumFileSize > 0 && fileProperties.Size > (ulong)maximumFileSize)
            {
                isFileSizeTooLarge = true;
                return new FileData(file.Path, file.Name, isFileSizeTooLarge, () => new MemoryStream());
            }
            else
            {
                return new FileData(file.Path, file.Name, isFileSizeTooLarge, () => file.OpenStreamForReadAsync().Result);
            }
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
    }
}