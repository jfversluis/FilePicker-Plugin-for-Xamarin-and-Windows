using System;

namespace Plugin.FilePicker.Abstractions
{
    public class FilePickerEventArgs : EventArgs
    {
        public byte [] FileByte { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public bool IsFileSizeTooLarge { get; set; }

        public FilePickerEventArgs ()
        {

        }

        public FilePickerEventArgs (ReadFileResult readFileResult)
        {
            FileByte = readFileResult.Data;
            IsFileSizeTooLarge = readFileResult.IsFileSizeTooLarge;
        }

        public FilePickerEventArgs (ReadFileResult readFileResult, string fileName)
            : this (readFileResult)
        {
            FileName = fileName;
        }

        public FilePickerEventArgs (ReadFileResult readFileResult, string fileName, string filePath)
            : this (readFileResult, fileName)
        {
            FilePath = filePath;
        }
    }
}