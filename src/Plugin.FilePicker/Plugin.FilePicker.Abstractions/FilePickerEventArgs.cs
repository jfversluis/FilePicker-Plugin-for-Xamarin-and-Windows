using System;

namespace Plugin.FilePicker.Abstractions
{
    /// <summary>
    /// Event arguments for the event when file picking was completed.
    /// </summary>
    public class FilePickerEventArgs : EventArgs
    {
        public byte[] FileByte { get; set; }

        /// <summary>
        /// File name part of picked file, without path
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Complete file path of picked file; on some OS this may contain an Uri
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Creates a new and empty file picker event args object
        /// </summary>
        public FilePickerEventArgs()
        {
        }

        public FilePickerEventArgs(byte[] fileByte)
        {
            this.FileByte = fileByte;
        }

        /// <summary>
        /// Creates new file picker event args
        /// </summary>
        /// <param name="fileByte">file bytes</param>
        /// <param name="fileName">file name part of picked file</param>
        public FilePickerEventArgs(byte[] fileByte, string fileName)
            : this(fileByte)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// Creates new file picker event args
        /// </summary>
        /// <param name="fileByte">file bytes</param>
        /// <param name="fileName">file name part of picked file</param>
        /// <param name="filePath">complete file path of picked file</param>
        public FilePickerEventArgs(byte[] fileByte, string fileName, string filePath)
            : this(fileByte, fileName)
        {
            this.FilePath = filePath;
        }
    }
}
