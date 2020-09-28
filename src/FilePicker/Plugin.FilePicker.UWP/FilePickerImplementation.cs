using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;


namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for file picking on UWP
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        /// <summary>
        /// Implementation for picking a file on UWP platform.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On UWP, specify a list of extensions, like this: ".jpg", ".png".
        /// </param>
        /// <returns>
        /// File data object, or null when user cancelled picking file
        /// </returns>
        public async Task<FileData> PickFile(string[] allowedTypes)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            if (allowedTypes != null)
            {
                var hasAtleastOneType = false;

                foreach (var type in allowedTypes)
                {
                    if (type.StartsWith("."))
                    {
                        picker.FileTypeFilter.Add(type);
                        hasAtleastOneType = true;
                    }
                }

                if (!hasAtleastOneType)
                {
                    picker.FileTypeFilter.Add("*");
                }
            }
            else
            {
                picker.FileTypeFilter.Add("*");
            }

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return null;
            }

            StorageApplicationPermissions.FutureAccessList.Add(file);
            return new FileData(file.Path, file.Name, () => file.OpenStreamForReadAsync().Result);
        }

        public async Task<FilePlaceholder> CreateOrOverwriteFile(string[] allowedTypes = null)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            };

            if (allowedTypes != null)
            {
                var hasAtleastOneType = false;

                for (var i = 0; i < allowedTypes.Length; i++)
                {
                    var type = allowedTypes[i];

                    if (type.StartsWith("."))
                    {
                        throw new Exception("Bad UWP string ordering");
                    }

                    var list = new List<string>();

                    for (int j = i + 1; j < allowedTypes.Length; j++)
                    {
                        var extension = allowedTypes[j];
                        if (!extension.StartsWith("."))
                        {
                            break;
                        }
                        else
                        {
                            list.Add(extension);
                            i = j;
                        }
                    }

                    if (list.Count == 0)
                    {
                        throw new Exception("Bad UWP string ordering");
                    }

                    picker.FileTypeChoices.Add(type, list);
                    hasAtleastOneType = true;
                }

                if (!hasAtleastOneType)
                {
                    picker.FileTypeChoices.Add("All Files", new List<string> { "*" });
                }
            }
            else
            {
                picker.FileTypeChoices.Add("All Files", new List<string> { "*" });
            }

            var file = await picker.PickSaveFileAsync();

            if (file == null) return null;

            var placeHolder = new FilePlaceholder(file.Path, file.Name, (stream, holder) => saveAction(stream, holder, file));

            return placeHolder;
        }

        private async Task saveAction(Stream stream, FilePlaceholder placeHolder, StorageFile file)
        {
            try
            {
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    await stream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }
            finally
            {
                placeHolder.Dispose();
            }
        }
    }
}