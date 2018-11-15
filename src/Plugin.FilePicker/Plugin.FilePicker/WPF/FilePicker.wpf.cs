using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.FilePicker
{
    static partial class FilePicker
    {
        static Task<FileData> PlataformPickFile(string[] allowedTypes)
        {
            Microsoft.Win32.OpenFileDialog picker = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

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

        static async Task<bool> PlataformSaveFile(FileData fileToSave)
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

        static void PlataformOpenFile(string fileToOpen)
        {
            try
            {
                if (File.Exists(fileToOpen))
                    Process.Start(fileToOpen);

            }
            catch (Exception)
            {
                // ignore exception
            }
        }

        static void PlataformOpenFile(FileData fileToOpen)
        {
            try
            {
                if (!File.Exists(fileToOpen.FileName))
                    SaveFile(fileToOpen).GetAwaiter().GetResult();

                Process.Start(fileToOpen.FileName);
            }
            catch (Exception)
            {
                // ignore exception
            }
        }

    }
}
