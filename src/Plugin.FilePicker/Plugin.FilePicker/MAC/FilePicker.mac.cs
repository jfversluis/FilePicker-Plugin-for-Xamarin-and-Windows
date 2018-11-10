using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AppKit;

namespace Plugin.FilePicker
{
    static partial class FilePicker
    {
        static Task<FileData> PlataformPickFile(string[] allowedTypes)
        {
            var openPanel = new NSOpenPanel
            {
                CanChooseFiles = true,
                AllowsMultipleSelection = false,
                CanChooseDirectories = false
            };

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
                    data = new FileData(path, fileName, () => File.OpenRead(path));
                }
            }

            return Task.FromResult(data);
        }

        static Task<bool> PlataformSaveFile(FileData fileToSave)
        {
            try
            {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var savePanel = new NSSavePanel
                {
                    Title = $"Save {fileToSave.FileName}",
                    CanCreateDirectories = true
                };

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

        static void PlataformOpenFile(string fileToOpen)
        {
            try
            {
                if (!NSWorkspace.SharedWorkspace.OpenFile(fileToOpen))
                    Debug.WriteLine($"Unable to open file at path: {fileToOpen}.");
                
            }
            catch (FileNotFoundException)
            {
                
            }
            catch (Exception)
            {
            }
        }

        static void PlataformOpenFile(FileData fileToOpen)
        {
            try
            {
                if (!NSWorkspace.SharedWorkspace.OpenFile(fileToOpen.FilePath))
                    Debug.WriteLine($"Unable to open file at path: {fileToOpen.FilePath}.");
                
            }
            catch (FileNotFoundException ex)
            {
                // this could be some strange UI behavior.
                // user would get prompted to save the file in order to open the file
                SaveFile(fileToOpen).GetAwaiter().GetResult();
                OpenFile(fileToOpen);
            }
            catch (Exception ex)
            {
            }
        }     
    }
}
