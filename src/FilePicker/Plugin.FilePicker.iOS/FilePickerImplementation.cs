using Foundation;
using MobileCoreServices;
using Plugin.FilePicker.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using System.Diagnostics;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for file picking on iOS
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        private readonly DocumentPicker _documentPicker = new DocumentPicker();

        public async Task<FileData> PickFile(string[] allowedTypes)
        {
            var fileData = await _documentPicker.PickMediaAsync(allowedTypes);

            return fileData;
        }

        public Task<FilePlaceholder> CreateOrOverwriteFile(string[] allowedTypes = null)
        {
            throw new NotImplementedException();
        }
    }
}