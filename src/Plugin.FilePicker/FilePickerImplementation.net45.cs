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
        /// File picker implementation for WPF; uses the Win32 SaveFileDialog from
        /// PresentationFoundation reference assembly.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On WPF, specify strings like this: "Data type (*.ext)|*.ext", which
        /// corresponds how the Windows file open dialog specifies file types.
        /// </param>
        /// <returns>file data of picked file, or null when picking was cancelled</returns>
        public Task<FileData> CreateOrOverwriteFile(string[] allowedTypes = null)
        {
            var picker = new Microsoft.Win32.SaveFileDialog();

            picker.InitialDirectory = 

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
    }
}
