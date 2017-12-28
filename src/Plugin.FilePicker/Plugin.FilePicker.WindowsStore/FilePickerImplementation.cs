using Plugin.FilePicker.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for FilePicker
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        public async Task<FileData> PickFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add("*");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var array = await ReadFile(file);

                return new FileData(file.Path, file.Name, () => file.OpenStreamForReadAsync().Result);
            }
            else
            {
                return null;
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
            catch (System.IO.FileNotFoundException ex)
            {
            }
            catch (System.Exception ex)
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
            catch (System.IO.FileNotFoundException ex)
            {
                await SaveFile(fileToOpen);
                OpenFile(fileToOpen);
            }
            catch (System.Exception ex)
            {
            }
        }

        public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes;

            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }
    }
}