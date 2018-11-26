using Plugin.FilePicker.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for file picking on WPF platform
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        /// <summary>
        /// File picker implementation for WPF; uses the Win32 OpenFileDialog from
        /// PresentationFoundation reference assembly.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On WPF, specify strings like this: "Data type (*.ext)|*.ext", which
        /// corresponds how the Windows file open dialog specifies file types.
        /// </param>
        /// <returns>file data of picked file, or null when picking was cancelled</returns>
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

            var fileName = Path.GetFileName(picker.FileName);

            var data = new FileData(picker.FileName, fileName, () => File.OpenRead(picker.FileName), (x) => { });

            return Task.FromResult(data);
        }

        /// <summary>
        /// WPF implementation of saving a picked file to a local folder.
        /// </summary>
        /// <param name="fileToSave">picked file data for file to save</param>
        /// <returns>true when file was saved successfully, false when not</returns>
        public async Task<bool> SaveFile(FileData fileToSave)
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

        /// <summary>
        /// WPF implementation of OpenFile(), opening a file already stored in a local folder.
        /// </summary>
        /// <param name="fileToOpen">relative filename of file to open</param>
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

        /// <summary>
        /// WPF implementation of OpenFile(), opening a picked file in an external viewer. The
        /// picked file is saved to a local folder before opening.
        /// </summary>
        /// <param name="fileToOpen">picked file data</param>
        public async void OpenFile(FileData fileToOpen)
        {
            try
            {
                if (!File.Exists(fileToOpen.FileName))
                {
                    await SaveFile(fileToOpen);
                }

                Process.Start(fileToOpen.FileName);
            }
            catch (Exception)
            {
                // ignore exception
            }
        }
    }
}
