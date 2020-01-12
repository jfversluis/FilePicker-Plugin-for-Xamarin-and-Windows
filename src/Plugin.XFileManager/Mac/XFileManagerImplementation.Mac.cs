using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using MobileCoreServices;


namespace Plugin.XFileManager
{
    public class FileManagerImplementation : NSObject, IXFileManager
    {
        public Task<FileData> PickFile(string[] allowedTypes)
        {
            // for consistency with other platforms, only allow selecting of a single file.
            // would be nice if we passed a "file options" to override picking multiple files & directories
            var openPanel = new NSOpenPanel();
            openPanel.CanChooseFiles = true;
            openPanel.AllowsMultipleSelection = false;
            openPanel.CanChooseDirectories = false;

            // macOS allows the file types to contain UTIs, filename extensions or a combination of the two.
            // If no types are specified, all files are selectable.
            if (allowedTypes != null)
            {
                openPanel.AllowedFileTypes = allowedTypes;
            }

            FileData data = null;

            var result = openPanel.RunModal();
            if (result == 1)
            {
                // Nab the first file
                var url = openPanel.Urls[0];

                if (url != null)
                {
                    var path = url.Path;
                    var fileName = Path.GetFileName(path);
                    var folderPath = Path.GetDirectoryName(path);
                    data = new FileData(path,folderPath,fileName, () => File.OpenRead(path));
                }
            }

            return Task.FromResult(data);
        }

        public Task<bool> SaveFile(FileData fileToSave, string folderPath)
        {
            try
            {
                var documents = folderPath;

                var savePanel = new NSSavePanel();
                savePanel.Title = $"Save {fileToSave.FileName}";
                savePanel.CanCreateDirectories = true;

                var result = savePanel.RunModal(documents, fileToSave.FileName);

                if (result == 1)
                {
                    var path = savePanel.Url.Path;

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

        public Task<string> PickFolder()
        {
            // for consistency with other platforms, only allow selecting of a single file.
            // would be nice if we passed a "file options" to override picking multiple files & directories
            var openPanel = new NSOpenPanel();
            openPanel.CanChooseFiles = false;
            openPanel.CanChooseDirectories = true;
            openPanel.AllowsMultipleSelection = false;

            FileData data = null;

            var result = openPanel.RunModal();
            if (result == 1)
            {
                // Nab the first folder
                var url = openPanel.Urls[0];

                if (url != null)
                {
                    var path = url.Path;
                    var folderPath = Path.GetDirectoryName(path);
                    return Task.FromResult(folderPath);
                }
                return Task.FromResult("");
            }
            return Task.FromResult("");


        }

        public void OpenFileViaEssentials(string fileToOpen)
        {
            try
            {
                if (!NSWorkspace.SharedWorkspace.OpenFile(fileToOpen))
                {
                    Debug.WriteLine($"Unable to open file at path: {fileToOpen}.");
                }
            }
            catch (FileNotFoundException)
            {
                // ignore exceptions
            }
            catch (Exception)
            {
                // ignore exceptions
            }
        }

        public string GetLocalAppFolder()
        {
            var libraryPath = GetPath(NSSearchPathDirectory.LibraryDirectory);
            var localPath = libraryPath;


            var process = System.Diagnostics.Process.GetCurrentProcess();
            var filename = System.IO.Path.GetFileNameWithoutExtension(process.MainModule.FileName);
            if (!string.IsNullOrWhiteSpace(filename))
            {
            localPath = System.IO.Path.Combine(libraryPath, "Application Support", filename);
            if (!System.IO.Directory.Exists(localPath))
            {
                System.IO.Directory.CreateDirectory(localPath);
            }
            }

            return localPath;
        }

        private string GetPath(NSSearchPathDirectory directory)
        {
            //We should only have one....
            return NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User)[0];
        }

        public Task<(bool, FileData)> GetFileDataFromPath(string filePath)
        {
            NSFileManager fileManager = NSFileManager.DefaultManager;
            if (fileManager.FileExists(filePath))
            {
                NSFileHandle nSFileHandle =  NSFileHandle.OpenRead(filePath);
                var fileData = nSFileHandle.AvailableData();

                return Task.FromResult((true, new FileData(filePath, Path.GetDirectoryName(filePath), Path.GetFileName(filePath), () => fileData.AsStream())));

            }
            else
            {
                return Task.FromResult<(bool,FileData)>((false, null));
            }

        }

        public Task<bool> SaveFileToLocalAppStorage(FileData fileToSave)
        {
            return SaveFile(fileToSave, GetLocalAppFolder());
        }

        public Task<bool> SaveFileInFolder(FileData fileToSave)
        {
            return SaveFile(fileToSave, fileToSave.FolderPath);
        }
    }
}
