using Plugin.FilePicker.Abstractions;
using System;
using System.Threading.Tasks;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for FilePicker
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        public void OpenFile(FileData fileToOpen)
        {
            throw new NotImplementedException();
        }

        public void OpenFile(string fileToOpen)
        {
            throw new NotImplementedException();
        }

        public Task<FileData> PickFile()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveFile(FileData fileToSave)
        {
            throw new NotImplementedException();
        }
    }
}