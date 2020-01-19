using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugin.XFileManager
{
    /// <summary>
    /// Implementation for file picking on WPF platform
    /// </summary>
    public class FileManagerImplementation : IXFileManager
    {
        public Task<FileData> PickFile(string[] allowedTypes = null)
        {
            Microsoft.Win32.OpenFileDialog picker = new Microsoft.Win32.OpenFileDialog();
            picker.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (allowedTypes != null)
            {
                picker.Filter = string.Join("|", allowedTypes);
            }

            var result = picker.ShowDialog();

            if (result == null || result == false)
            {
                return Task.FromResult<FileData>(null);
            }

            var folderPath = Path.GetDirectoryName(picker.FileName);
            var fileName = Path.GetFileName(picker.FileName);
            var fullPath = picker.FileName;

            var data = new FileData(fullPath, fileName, () => File.OpenRead(picker.FileName), (x) => { });

            return Task.FromResult(data);
        }

        public void OpenFile(string fileToOpen)
        {
            try
            {
                if (File.Exists(fileToOpen))
                {
                    Process.Start(fileToOpen);
                }
            }
            catch (Exception)
            {
                // ignore exception
            }
        }

        public async void OpenFile(FileData fileToOpen)
        {
            OpenFile(fileToOpen.FilePath);
        }

        public Task<FolderData> PickFolder()
       {

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
            var result = folderBrowserDialog.ShowDialog();




            if (result == DialogResult.Cancel)
            {
                return Task.FromResult<FolderData>(null);
            }

            var folder = new FolderData()
            {
                FolderPath = folderBrowserDialog.SelectedPath,
                FolderName = Path.GetDirectoryName(folderBrowserDialog.SelectedPath)
            };

            return Task.FromResult(folder);
        }

        public async Task<bool> SaveFileToLocalAppStorage(FileData fileToSave)
        {
            try
            {
                var localFolder = GetLocalAppFolder();
                using (FileStream sourceStream = new FileStream(localFolder + fileToSave.FileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await sourceStream.WriteAsync(fileToSave.DataArray, 0, fileToSave.DataArray.Length);
                }

                return true;
            }
            catch (Exception)
            {
                // ignore exception
                return false;
            }
        }

        public async Task<bool> SaveFileInFolder(FileData fileToSave, FolderData folder)
        {
            try
            {
                using (FileStream sourceStream =
                    new FileStream(fileToSave.FilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await sourceStream.WriteAsync(fileToSave.DataArray, 0, fileToSave.DataArray.Length);
                }

                return true;
            }
            catch (Exception)
            {
                // ignore exception
                return false;
            }
        }

        public async Task<bool> OpenFileViaEssentials(string fileToOpen)
        {
            OpenFile(fileToOpen);
            return true;
        }

        public FolderData GetLocalAppFolder()
        {
            var folder = new FolderData()
            {
                FolderName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                FolderPath = Path.GetDirectoryName(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData))
            };
            return folder;

        }

        public async Task<(bool, FileData)> GetFileDataFromPath(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return (true, new FileData(filePath, Path.GetFileName(filePath), ()=> File.OpenRead(filePath)));
                }
                else
                {
                    return (false, null);
                }
            }
            catch (Exception)
            {
                return (false, null);
                // ignore exception
            }
        }
    }
}
