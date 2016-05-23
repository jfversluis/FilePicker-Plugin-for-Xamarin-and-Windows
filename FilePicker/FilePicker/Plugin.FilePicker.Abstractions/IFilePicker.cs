using System;
using System.Threading.Tasks;

namespace Plugin.FilePicker.Abstractions
{
    /// <summary>
    /// Interface for FilePicker
    /// </summary>
    public interface IFilePicker
    {
        Task<FileData> PickFile();
    }

    public class FileData
    {
        public byte[] DataArray { get; set; }

        public string FileName { get; set; }
    }

    public class FilePickerEventArgs : EventArgs
    {
        public byte[] FileByte { get; set; }

        public string FileName { get; set; }
        public FilePickerEventArgs()
        {

        }

        public FilePickerEventArgs(byte[] fileByte)
        {
            FileByte = fileByte;
        }

        public FilePickerEventArgs(byte[] fileByte, string fileName)
        {
            FileByte = fileByte;
            FileName = fileName;
        }


    }
}
