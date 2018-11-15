using System;
using System.Threading.Tasks;

namespace Plugin.FilePicker
{
    static partial class FilePicker
    {
        static Task<FileData> PlataformPickFile(string[] allowedTypes) => throw new NotImplementedException();

        static Task<bool> PlataformSaveFile(FileData fileToSave) => throw new NotImplementedException();

        static void PlataformOpenFile(string fileToOpen) => throw new NotImplementedException();

        static void PlataformOpenFile(FileData fileToOpen) => throw new NotImplementedException();
    }
}
