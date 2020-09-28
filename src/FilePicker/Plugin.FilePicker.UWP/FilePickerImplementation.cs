using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;

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
                IList<string> list;
                var fileType = "";

                foreach (var type in allowedTypes)
                {
                    if (!type.StartsWith("."))
                    {
                        fileType = type;

                        if (string.IsNullOrEmpty(fileType) || picker.FileTypeChoices.ContainsKey(fileType))
                        {
                            throw new Exception("Bad UWP string ordering");
                        }

                        picker.FileTypeChoices.Add(fileType, new List<string>());
                    }
                    else if(!string.IsNullOrEmpty(fileType) && picker.FileTypeChoices.ContainsKey(fileType))
                    {
                        picker.FileTypeChoices[fileType].Add(type);
                        hasAtleastOneType = true;
                    }
                    else
                    {
                        throw new Exception("Bad UWP string ordering");
                    }
                }

                if (!hasAtleastOneType)
                {
                    picker.FileTypeChoices.Add("All Files", new List<string>{"*"});
                }
            }
            else
            {
                picker.FileTypeChoices.Add("All Files", new List<string> { "*" });
            }

            var file = await picker.PickSaveFileAsync();

            if (file == null) return null;

            var placeHolder = new FilePlaceholder(file.Path, file.Name, saveAction);

            return placeHolder;
        }

        private void saveAction(Stream stream, FilePlaceholder placeholder)
        {
            using (var fileStream = File.Create(placeholder.FilePath))
            {
                stream.CopyTo(fileStream);
                fileStream.Flush();
            }
        }
    }
}