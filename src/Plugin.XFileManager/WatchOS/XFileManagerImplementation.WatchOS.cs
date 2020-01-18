using System;
using System.IO;
using System.Threading.Tasks;

//[assembly: Xamarin.Forms.Dependency(typeof(Plugin.UWP.FileManagerImplementation))]
namespace Plugin.XFileManager
{
    /// <summary>
    /// Implementation for file picking on UWP
    /// </summary>
    class FileManagerImplementation : IXFileManager
    {
        public Task<(bool, FileData)> GetFileDataFromPath(string filePath)
        {
            throw new NotImplementedException();
        }

        public string GetLocalAppFolder()
        {
            throw new NotImplementedException();
        }

        public Task<bool> OpenFileViaEssentials(string fileToOpen)
        {
            throw new NotImplementedException();
        }

        public Task<FileData> PickFile(string[] allowedTypes = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> PickFolder()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveFileInFolder(FileData fileToSave)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveFileToLocalAppStorage(FileData fileToSave)
        {
            throw new NotImplementedException();
        }
    }
}
