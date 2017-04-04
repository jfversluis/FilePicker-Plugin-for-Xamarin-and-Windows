using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using MobileCoreServices;
using Plugin.FilePicker.Abstractions;

namespace Plugin.FilePicker
{
    public class FilePickerImplementation : NSObject, IFilePicker
    {
        private int _requestId;
        private TaskCompletionSource<FileData> _completionSource;

        /// <summary>
        /// Event which is invoked when a file was picked
        /// </summary>
        public EventHandler<FilePickerEventArgs> Handler
        {
            get;
            set;
        }

        public async Task<FileData> PickFile()
        {
            var media = await TakeMediaAsync();

            return media;
        }

        private Task<FileData> TakeMediaAsync()
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time");

            var allowedUtis = new string[] {
                UTType.UTF8PlainText,
                UTType.PlainText,
                UTType.RTF,
                UTType.PNG,
                UTType.Text,
                UTType.PDF,
                UTType.Image,
                UTType.UTF16PlainText,
                UTType.FileURL
            };

            var dlg = NSOpenPanel.OpenPanel;
            dlg.CanChooseFiles = true;
            dlg.CanChooseDirectories = false;
            //dlg.AllowedFileTypes = new string[] { "txt", "html", "md", "css" };

            FileData data = null;

            //await Task.Factory.StartNew(() =>
            //{
            var result = dlg.RunModal();
            if (result == 1)
            {
                // Nab the first file
                var url = dlg.Urls[0];
                var fileName = dlg.Filenames[0];

                if (url != null)
                {
                    var path = url.Path;
                    //url.fr
                    data = new FileData(path, fileName, () => File.OpenRead(path));
                }
            }
            //});

            return Task.FromResult(data);
        }

        public Task<bool> SaveFile(FileData fileToSave)
        {
            try
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var dlg = NSSavePanel.SavePanel;
                dlg.Title = $"Save {fileToSave.FileName}";
                dlg.CanCreateDirectories = true;
                //dlg.ShouldShowFilename = 
                //dlg.Filename = fileToSave.FileName;

                var result = dlg.RunModal(documents, fileToSave.FileName);

                if (result == 1)
                {
                    var path = dlg.Url.Path;

                    File.WriteAllBytes(path, fileToSave.DataArray);

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Task.FromResult(false);
            }
        }

        public void OpenFile(string fileToOpen)
        {
            if (!NSWorkspace.SharedWorkspace.OpenFile(fileToOpen))
            {
                Debug.WriteLine($"Unable to open file \"{fileToOpen}\"");
            }
        }

        public void OpenFile(FileData fileToOpen)
        {
            string fileUrl = fileToOpen.FileName;

            if (fileToOpen.FilePath != null &&
               !fileToOpen.FilePath.EndsWith(fileToOpen.FileName, StringComparison.CurrentCulture) &&
               !fileToOpen.FilePath.Equals(fileToOpen.FileName))
            {
                fileUrl = string.Format("{0}/{1}", fileToOpen.FilePath, fileToOpen.FileName);
            }

            OpenFile(fileToOpen.FileName);
        }

        private int GetRequestId()
        {
            var id = _requestId;

            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }
    }
}
