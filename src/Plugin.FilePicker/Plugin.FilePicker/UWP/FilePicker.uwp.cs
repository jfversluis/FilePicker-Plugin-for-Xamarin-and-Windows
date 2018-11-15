using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;


namespace Plugin.FilePicker
{
    static partial class FilePicker
    {
        static async Task<FileData> PlataformPickFile(string[] allowedTypes)
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
                    picker.FileTypeFilter.Add("*");

            }
            else
                picker.FileTypeFilter.Add("*");


            var file = await picker.PickSingleFileAsync();
            if (file is null)
                return null;

            StorageApplicationPermissions.FutureAccessList.Add(file);
            return new FileData(file.Path, file.Name, () => file.OpenStreamForReadAsync().GetAwaiter().GetResult());
        }

        static async Task<bool> PlataformSaveFile(FileData fileToSave)
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

        static void PlataformOpenFile(string fileToOpen)
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFileAsync(fileToOpen).AsTask().GetAwaiter().GetResult();

                if (file != null)
                {
                    Launcher.LaunchFileAsync(file).AsTask().GetAwaiter().GetResult();
                }
            }
            catch (FileNotFoundException ex)
            {
            }
            catch (Exception ex)
            {
            }
        }

        static void PlataformOpenFile(FileData fileToOpen)
        {
            try
            {
                var file = ApplicationData.Current.LocalFolder.GetFileAsync(fileToOpen.FileName).AsTask().GetAwaiter().GetResult();

                if (!(file is null))
                {
                    Launcher.LaunchFileAsync(file).AsTask().GetAwaiter().GetResult();
                }
            }
            catch (FileNotFoundException ex)
            {
                SaveFile(fileToOpen).GetAwaiter().GetResult();
                OpenFile(fileToOpen);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
