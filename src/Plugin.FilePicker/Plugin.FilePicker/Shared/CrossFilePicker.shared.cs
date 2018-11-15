using System.Threading.Tasks;

namespace Plugin.FilePicker
{
    public static class CrossFilePicker
    {
        public static Task<FileData> PickFile(string[] allowedTypes = null) => PlataformPickFile(allowedTypes);

        public static Task<bool> SaveFile(FileData fileToSave) => PlataformSaveFile(fileToSave);

        public static void OpenFile(string fileToOpen) => PlataformOpenFile(fileToOpen);

        public static void OpenFile(FileData fileToOpen) => PlataformOpenFile(fileToOpen);
    }
}
