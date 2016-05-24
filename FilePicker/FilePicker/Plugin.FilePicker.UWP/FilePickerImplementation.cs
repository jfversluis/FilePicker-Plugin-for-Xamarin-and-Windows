using Plugin.FilePicker.Abstractions;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for Feature
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

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var array = await ReadFile(file);

                return new FileData
                {
                    DataArray = array,
                    FileName = file.Name
                };
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

				await FileIO.WriteBytesAsync(file,fileToSave.DataArray);

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

	

		public void OpenFile(string fileToOpen)
		{

			try {
				var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileToOpen);

				if(file!=null){
					await Launcher.LaunchFileAsync(file)
				}
			}
			catch (System.IO.FileNotFoundException ex) {
				
			}
			catch (System.Exception ex) {
				
			}
		}

		public async void OpenFile(FileData fileToOpen)
		{
			try {
				var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileToOpen.FileName);

				if(file!=null){
					await Launcher.LaunchFileAsync(file)
				}
			}
			catch (System.IO.FileNotFoundException ex) {
				await SaveFile(fileToOpen);
				OpenFile(fileToOpen);
			}
			catch (System.Exception ex) {
				
			}
		}

        public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }
    }
}